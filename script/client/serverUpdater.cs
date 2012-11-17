//================================================
// Title: Glass Server Updater Client
//================================================

if(!isObject(BLG_SUC)) {
	new ScriptObject(BLG_SUC);
}

BLG_GSC.registerRelayHandle("updates", "BLG_SUC.onLine");

function BLG_SUC::openUpdates(%this, %id) {
	canvas.pushDialog(BLG_ServerUpdates);
	%this.displaying = %id;
	BLG_GSC.relay(%id, "updates\tgetupdates");
}

function BLG_SUC::update(%this, %name) {
	//BLG_GSC.relay(%this.displaying, "updates\tconfirm\t" @ %name);
}

function BLG_SUC::onLine(%this, %sender, %msg) {
	BLG_ServerUpdates_Scroll.clear();
	if(%sender = %this.displaying) {
		%data = strReplace(strReplace(getSubStr(%msg, strLen(getWord(%msg, 0))+1, strLen(%msg)), "<->", "\n"), "<.>", "\t");
		echo(%data);

		%y = 0;
		for(%i = 0; %i < getLineCount(%data); %i++) {
			%line = getLine(%data, %i);
			%gui = new GuiSwatchCtrl() {
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

				new GuiBitmapButtonCtrl() {
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
					bitmap = "Add-Ons/System_ReturnToBlockland/images/icons/rtbLogo";
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