//================================================
// Title: Glass Gui Server
//================================================

if(!isObject(BLG_GDS)) {
	new ScriptObject(BLG_GDS) {
		sg = "";
	};
	BLG_GDS.defaultHud[%i++] = "LagIcon";
	BLG_GDS.defaultHud[%i++] = "HUD_PlantError";
	BLG_GDS.defaultHud[%i++] = "GuiHealthBarHud";
	BLG_GDS.defaultHud[%i++] = "Crosshair";
	BLG_GDS.defaultHud[%i++] = "HUD_PlantErrorSmall";
	BLG_GDS.defaultHud[%i++] = "HudInvBox";
	BLG_GDS.defaultHud[%i++] = "HUD_EnergyBar";
	BLG_GDS.defaultHud[%i++] = "MouseToolTip";
	BLG_GDS.defaultHud[%i++] = "HUD_SuperShift";
	BLG_GDS.defaultHud[%i++] = "HUD_EnergyBar";
	BLG_GDS.defaultHud[%i++] = "centerPrintDlg";
	BLG_GDS.defaultHud[%i++] = "bottomPrintDlg";
	BLG_GDS.defaultHud[%i++] = "HUD_Ghosting";
	BLG_GDS.defaultHud[%i++] = "HUD_BrickBox";
	BLG_GDS.defaultHud[%i++] = "HUD_BrickNameBG";
	BLG_GDS.defaultHud[%i++] = "HUD_PaintBox";
	BLG_GDS.defaultHud[%i++] = "HUD_PaintNameBG";
	BLG_GDS.defaultHud[%i++] = "HUD_ToolBox";
	BLG_GDS.defaultHud[%i++] = "HUD_ToolNameBG";
	BLG_GDS.defaultHuds = %i;

	new ScriptGroup(BLG_Objects) {
		objs = 0;
	};

	BLG_GDS.sg = BLG_Objects;
} 

function BLG_GDS::registerObject(%this, %obj) {
	if(isObject(%obj)) {
		if(!BLG_Objects.isMember(%obj)) {

			%so = new ScriptObject() {
				class = BLG_GuiObject;

				attributes = 0;
				baseObj = %obj;
				id = BLG_Objects.objs;
				children = "";
				parent = "";
				sent = false;

				handlers = 0;
				altHandlers = 0;
				closeHandlers = 0;
			};

			BLG_Objects.obj[BLG_Objects.objs] = %so;
			BLG_Objects.add(%so);
			BLG_Objects.objs++;

			%so.getAttributes();

			//init child registration
			for(%i = 0; %i < %obj.getCount(); %i++) {
				%ob = %obj.getObject(%i);
				%sob = %this.registerObject(%ob);
				%so.children = trim(%so.children SPC %sob.id);
				%sob.parent = %so.id;
			}

			return %so;
		}
	}
}

function BLG_GDS::getDataObject(%this, %object) {
	for(%i = 0; %i < BLG_Objects.objs; %i++) {
		%obj = BLG_Objects.obj[%i];
		if(%obj.baseObj.getId() == %object.getId()) {
			return %obj;
		}
	}
}

//================================================
// BLG_GuiObject
//================================================

function BLG_GuiObject::getAttributes(%this) {
	if(isObject(%this.baseObj)) {
		%obj = %this.baseObj;
		%file = "config/BLG/server/temp/gui.txt";
		if(isFile(%file)) {
			fileDelete(%file);
		}
		%obj.save(%file);

		%fo = new FileObject();
		%fo.openForRead(%file);
		while(!%fo.isEOF()) {
			%line = trim(%fo.readLine());
			if(strPos(%line, "new ") == 0) {
				if(%started) {
					break;
				} else {
					%started = true;
					continue;
				}
			} else if(strPos(%line, "};") == 0) {
				break;
			} else if(%line $= "") {
				continue;
			} else if(%started) {
				%data = getSubStr(%line, 0, strPos(%line, " = "));
				%value = getSubStr(%line, strPos(%line, " = ")+3, strLen(%line));
				%value = strReplace(%value, "\\\"", "[[quotation]]");
				%value = strReplace(%value, "\"", "");
				%value = strReplace(%value, "[[quotation]]", "\\\"");
				%value = getSubStr(%value, 0, strLen(%value)-1);

				if(%data $= "command" || %data $= "altCommand" || %data $= "closeCommand") {
					continue;
				}

				if(getSubStr(%value, 0, 1) $= "~") {
					%value = "config" @ getSubStr(%value, 1, strlen(%value));
				}

				%this.attributeData[%this.attributes] = %data;
				%this.attributeValue[%this.attributes] = %value;
				%this.attributes++;
			}
		}
		%fo.close();
		%fo.delete();
		fileDelete(%file);
	}
}

function BLG_GuiObject::transfer(%this, %client) {
	%time = 0;
	%delay = 50;

	%this.schedule(%time+=%delay, "send", %client, "0" TAB %this.id TAB %this.baseObj.getClassName() TAB %this.baseObj.getName() TAB (%this.parent $= ""));

	for(%i = 0; %i < %this.attributes; %i++) {
		%this.schedule(%time+=%delay, "send", %client, "1" TAB %this.id TAB %this.attributeData[%i] TAB %this.attributeValue[%i]);
	}
	%this.sent = true;
	BLG_GDS.schedule(%time+=%delay, "sendNextObject", %client, %this.id);
}

function BLG_GuiObject::send(%this, %client, %msg) {
	commandtoclient(%client, 'BLG_ObjectInfo', %msg);
}

function BLG_GuiObject::updateAttribute(%this, %client, %data, %value) {
	if(%this.sent) {
		if(%data $= "command" || %data $= "altCommand" || %data $= "closeCommand") {
			return;
		}

		%this.send(%client, "1" TAB %this.id TAB %data TAB %value);
	}
}

function BLG_GuiObject::pop(%this, %client) {
	%this.send(%client, "5" TAB %this.id TAB "0");
}

function BLG_GuiObject::push(%this, %client) {
	%this.send(%client, "5" TAB %this.id TAB "1");
}

function BLG_GuiObject::registerHandler(%this, %call) {
	%this.handler[%this.handlers] = %call;
	%this.handlers++;
}

function BLG_GuiObject::registerAltHandler(%this, %call) {
	%this.altHandler[%this.altHandlers] = %call;
	%this.altHandlers++;
}

function BLG_GuiObject::registerCloseHandler(%this, %call) {
	%this.closeHandler[%this.closeHandlers] = %call;
	%this.closeHandlers++;
}

function BLG_GuiObject::setValue(%this, %client, %value) {
	%this.send(%client, "8" TAB %this.id TAB %value);
	%client.BLG_valueCache[%objId] = %value;
}

function BLG_GuiObject::addRow(%this, %client, %index, %text) {
	%this.rows++;
	%this.send(%client, "9" TAB %this.id TAB %index TAB %text);
}

function BLG_GuiObject::removeRow(%this, %client, %index) {
	%this.rows--;
	%this.send(%client, "10" TAB %this.id TAB %index);
}

function BLG_GuiObject::PopUpAdd(%this, %client, %text, %index) {
	%this.send(%client, "11" TAB %this.id TAB %text TAB %index);
}

function BLG_GuiObject::clear(%this, %client) {
	%this.send(%client, "12", %this.id);
}

function BLG_GuiObject::getValue(%this, %client) {
	return %client.BLG_valueCache[%this.id];
}

function BLG_GuiObject::setAsHud(%this, %client, %tog) {
	%this.send(%client, "13" TAB %this.id TAB %tog);
}

function BLG_GuiObject::setDefaultHudOverride(%this, %client, %hud) {
	for(%i = 0; %i < BLG_GDS.defaultHuds; %i++) {
		if(BLG_GDS.defaultHud[%i] $= %hud) {
			%legit = true;
			break;
		}
	}
	if(%legit) {
		%this.send(%client, "14" TAB %this.id TAB %hud);
	} else {
		BLG.debug("Tried to register bad hud override", 0);
	}
}

function serverCmdBLG_GuiReturn(%client, %msg) {
	BLG.debug("GuiReturn: [" @ %msg @ "]");
	%funcId = getField(%msg, 0);
	%objId = getField(%msg, 1);

	switch(%funcid) {
		case 0: //Callback
			%obj = BLG_Objects.obj[%objId];
			%type = getField(%msg, 2);
			if(%type $= "command") {
				for(%i = 0; %i < %obj.handlers; %i++) {
					%call = %obj.handler[%i];
					eval(%call @ "(" @ %client @ "," @ %obj @ ");");
				}
			} else if(%type $= "alt") {
				for(%i = 0; %i < %obj.altHandlers; %i++) {
					%call = %obj.altHandler[%i];
					eval(%call @ "(" @ %client @ "," @ %obj @ ");");
				}
			} else if(%type $= "close") {
				for(%i = 0; %i < %obj.closeHandlers; %i++) {
					%call = %obj.closeHandler[%i];
					eval(%call @ "(" @ %client @ "," @ %obj @ ");");
				}
			}

		case 1: //Set cache value 
			%obj = BLG_Objects.obj[%objId];
			%client.BLG_valueCache[%objId] = getField(%msg, 2);
	}
}

//================================================
// Mission Prepare Phase stuff
//================================================

function BLG_GDS::startTransfer(%this, %client) {
	if(BLG_Objects.objs > 0) {
		%obj = BLG_Objects.obj[0];
		if(isObject(%obj)) {
			%obj.transfer(%client);
		}
	} else {
		%this.transferFinished(%client);
	}
}

function BLG_GDS::sendNextObject(%this, %client, %objId) {
	if(isObject(BLG_Objects.obj[%objId+1])) {
		BLG_Objects.obj[%objId+1].transfer(%client);
	} else {
		%time = 0;
		%delay = 5;
		for(%i = 0; %i < BLG_Objects.objs; %i++) {
			%obj = BLG_Objects.obj[%i];
			if(%obj.parent !$= "") {
				%obj.schedule(%time+=%delay, "send", %client, "2" TAB %obj.parent TAB %obj.id);
			}
		}
		%this.schedule(%time+=%delay, "transferFinished", %client);
	}
} 

function serverCmdMissionPreparePhaseBLGAck(%client) {
	BLG_GDS.startTransfer(%client);
}

function BLG_GDS::transferFinished(%this, %client) {
	%client.currentPhase = 0;
	%client.BLG_DownloadedGUI = true;
	commandToClient(%client,'BLG_guiTransferFinished');
	if(%client.BLGVersionId >= 2) {
		BLG_IDS.startPhase(%client);
	} else {
		commandToClient(%client,'MissionStartPhase1', $missionSequence, $Server::MissionFile);
	}
}

function BLG_GDS::getPartCount(%this) {
	%parts = 0;
	for(%i = 0; %i < BLG_Objects.getCount(); %i++) {
		%obj = BLG_Objects.getObject(%i);
		%parts += 1; //Initiation
		%parts += %obj.attributes;
		if(%obj.parent !$= "") {
			%parts += 1;
		}
	}
	return %parts;
}