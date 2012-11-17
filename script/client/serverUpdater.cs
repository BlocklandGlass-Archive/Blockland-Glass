//================================================
// Title: Glass Server Updater Client
//================================================

if(!isObject(BLG_SUC)) {
	new ScriptObject(BLG_SUC);
}

BLG_GSC.registerRelayHandle("updates", "BLG_SUC.onLine");
BLG_GSC.registerHandle("server", "BLG_SUC.server");

function BLG_SUC::openUpdates(%this, %id) {
	//LG_GNS.updateNotification("updates_" @ %id, "Server Updates", "A server of yours has available updates!", "wrench", 1);
	canvas.pushDialog(BLG_ServerUpdates);
	%this.displaying = %id;
	BLG_GSC.relay(%id, "updates\tgetupdates");
}

function BLG_SUC::update(%this, %name) {
	%button = "BLG_ServerUpdates_Button_" @ %name;
	//%button.enabled = false;
	%button.mColor = "128 128 128 255";
	BLG_GSC.relay(%this.displaying, "updates\tconfirm\t" @ %name);
	%this.schedule(1000, removeItem, %name);
}

function BLG_SUC::updateAll(%this) {
	for(%i = 0; %i < BLG_ServerUpdates_Scroll.getCount(); %i++) {
		%obj = BLG_ServerUpdates_Scroll.getObject(%i);
		%this.update(%obj.name);
	}
}

function BLG_SUC::removeItem(%this, %name) {
	for(%i = 0; %i < BLG_ServerUpdates_Scroll.getCount(); %i++) {
		%obj = BLG_ServerUpdates_Scroll.getObject(%i);
		if(%obj.name $= %name) {
			%pos = getWord(%obj.position, 1);
			break;
		}
	}

	BLG_ServerUpdates_Scroll.remove(%obj);

	for(%i = 0; %i < BLG_ServerUpdates_Scroll.getCount(); %i++) {
		%obj = BLG_ServerUpdates_Scroll.getObject(%i);
		if(getWord(%obj.position, 1) > %pos) {
			%obj.position = 0 SPC getWord(%obj.position, 1)-36;
		}
	}
	BLG_ServerUpdates_Scroll.extent = "263" SPC getWord(BLG_ServerUpdates_Scroll.extent, 1)-36;
	return;
	canvas.popDialog(BLG_ServerUpdates);
	canvas.pushDialog(BLG_ServerUpdates);
}

function BLG_SUC::server(%this, %msg) {
	switch$(getField(%msg, 1)) {
		case "serverlist":
			%this.serverList = getField(%msg, 2);
			for(%i = 0; %i < getWordCount(getField(%msg, 2)); %i++) {
				%id = getWord(getField(%msg, 2), %i);
				BLG_GSC.relay(%id, "updates\tcheckupdates");
			}
		case "add":
			%this.serverList = %this.serverList SPC getField(%msg, 2);
			BLG_GSC.relay(getField(%msg, 2), "updates\tcheckupdates");
		case "remove":
			%this.serverList = trim(strReplace(strReplace(%this.serverList, getField(%msg, 2), ""), "  ", " "));

	}
}

function BLG_SUC::onLine(%this, %sender, %msg) {
	BLG_ServerUpdates_Scroll.clear();
	if(%sender = %this.displaying) {
		%data = strReplace(strReplace(getSubStr(%msg, strLen(getWord(%msg, 0))+1, strLen(%msg)), "<->", "\n"), "<.>", "\t");

		%y = 0;
		for(%i = 0; %i < getLineCount(%data); %i++) {
			%line = getLine(%data, %i);
			if(getField(%line, 2) == 0) {
				%image = "Add-Ons/System_ReturnToBlockland/images/icons/rtbLogo";
			} else if(getField(%line, 2) == 1) {
				%image = "Add-Ons/System_BlocklandGlass/image/logo.jpg";
			} else {
				%image = "";
			}
			%gui = new GuiSwatchCtrl() {
				name = getField(%line, 0);
				profile = "GuiDefaultProfile";
				horizSizing = "right";
				vertSizing = "bottom";
				position = "0" SPC %y;
				extent = "263 30";
				minExtent = "8 2";
				enabled = "1";
				visible = "1";
				clipToParent = "1";
				color = "128 128 128 50";

				new GuiBitmapButtonCtrl("BLG_ServerUpdates_Button_" @ getField(%line, 0)) {
					profile = "BLG_BlockButtonProfile";
					horizSizing = "right";
					vertSizing = "center";
					position = "208 5";
					extent = "50 20";
					minExtent = "8 2";
					enabled = "1";
					visible = "1";
					clipToParent = "1";
					text = "Update";
					groupNum = "-1";
					buttonType = "PushButton";
					bitmap = "base/client/ui/button1";
					lockAspectRatio = "0";
					alignLeft = "0";
					alignTop = "0";
					overflowImage = "0";
					mKeepCached = "0";
					mColor = "255 255 255 255";
					command = "BLG_SUC.update(\"" @ getField(%line, 0) @ "\");";
				};
				new GuiBitmapCtrl() {
					profile = "GuiDefaultProfile";
					horizSizing = "right";
					vertSizing = "center";
					position = "7 7";
					extent = "16 16";
					minExtent = "8 2";
					enabled = "1";
					visible = "1";
					clipToParent = "1";
					bitmap = %image;
					wrap = "0";
					lockAspectRatio = "1";
					alignLeft = "0";
					alignTop = "0";
					overflowImage = "0";
					keepCached = "0";
					mColor = "255 255 255 255";
					mMultiply = "0";
				};
				new GuiTextCtrl() {
					profile = "GuiTextProfile";
					horizSizing = "right";
					vertSizing = "bottom";
					position = "31 6";
					extent = "57 18";
					minExtent = "8 2";
					enabled = "1";
					visible = "1";
					clipToParent = "1";
					text = getField(%line, 0) SPC "(" @ getField(%line, 1) @ ")";
					maxLength = "255";
				};
			};
			BLG_ServerUpdates_Scroll.add(%gui);
			%y += 36;
			BLG_ServerUpdates_Scroll.extent = "300" SPC %y;
		}
		canvas.popDialog(BLG_ServerUpdates);
		canvas.pushDialog(BLG_ServerUpdates);
	}
}