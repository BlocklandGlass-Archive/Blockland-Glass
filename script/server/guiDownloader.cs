//================================================
// Title: Glass Gui Client 
//================================================

if(!isObject(BLG_GDS)) {
	new ScriptObject(BLG_GDS) {
		sg = "";
	};

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
			};

			BLG_Objects.obj[BLG_Objects.objs] = %so;
			BLG_Objects.add(%so);
			BLG_Objects.objs++;

			%so.getAttributes();
			return %so;
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
	%delay = 5;

	%this.schedule(%time+=%delay, "send", %client, "0" TAB %this.id TAB %this.baseObj.getClassName() TAB %this.baseObj.getName() TAB (%this.parent $= ""));
	%this.schedule(%time+=%delay, "send", %client, "1" TAB %this.id TAB "command" TAB %this.specialValue["command"]);
	%this.schedule(%time+=%delay, "send", %client, "1" TAB %this.id TAB "altCommand" TAB %this.specialValue["altCommand"]);
	%this.schedule(%time+=%delay, "send", %client, "1" TAB %this.id TAB "closeCommand" TAB %this.specialValue["closeCommand"]);

	for(%i = 0; %i < %this.attributes; %i++) {
		%this.schedule(%time+=%delay, "send", %client, "1" TAB %this.id TAB %this.attributeData[%i] TAB %this.attributeValue[%i]);
	}

	BLG_GDS.schedule(%time+=%delay, "sendNextObject", %client, %this.id);
}

function BLG_GuiObject::send(%this, %client, %msg) {
	commandtoclient(%client, 'BLG_ObjectInfo', %msg);
}

function BLG_GuiObject::updateAttribute(%this, %client, %data, %value) {

}

//================================================
// Mission Prepare Phase stuff
//================================================

function BLG_GDS::startTransfer(%this, %client) {
	echo("start transfer");
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
	echo(%objId @ " finished, checking for " @ %objId+1);
	if(isObject(BLG_Objects.obj[%objId+1])) {
		BLG_Objects.obj[%objId+1].transfer(%client);
	} else {
		%time = 0;
		%delay = 5;
		for(%i = 0; %i < BLG_Objects.getCount(); %i++) {
			%obj = BLG_Objects.getObject(%i);
			if(%obj.parent !$= "") {
				%this.schedule(%time+=%delay, "send", %client, "2" TAB %obj.parent TAB %obj.id);
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
	commandToClient(%client,'MissionStartPhase1', $missionSequence, $Server::MissionFile);
}

function BLG_GDS::getPartCount(%this) {
	%parts = 0;
	for(%i = 0; %i < BLG_Objects.getCount(); %i++) {
		%obj = BLG_Objects.getObject(%i);
		%parts += 4; //Initiation, command values
		%parts += %obj.attributes;
	}
	return %parts;
}