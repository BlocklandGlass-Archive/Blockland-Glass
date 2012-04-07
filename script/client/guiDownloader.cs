//================================================
// Title: Glass Gui Client 
//================================================

if(!isObject(BLG_GDC)) {
	new ScriptObject(BLG_GDC){ //Main Object
		SG = ""; //Clean-up group for disconnect
	};
	BLG_GDC.SG = new ScriptGroup();
}

//Protocol:
//----------
// - Register Objects
// - Register Attributes
// - Register Subclasses
// - Finalize parent objects

function BLG_GDC::verifyString(%this, %string) { //Checks sent message to make sure it has no unwanted code
	%illegal = ";.";

	for(%i = 0; %i < strLen(%illegal); %i++) {
		if(strPos(%string, getSubStr(%illegal, %i, 1)) != -1) {
			return false;
		}
	}

	for(%i = 0; %i < strLen(%string); %i++) {
		%char = getSubStr(%string, %i, 1);
		if(%char $= "\"") {
			if(getSubStr(%string, %i-1, 1) $= "\\") {
				//false alarm
			} else {
				return false;
			}
		}
	}

	return true;
}

function BLG_GDC::verifyAlphabetic(%this, %string) {
	%allowed = "abcdefghijklmnopqrstuvwxyz";
	for(%i = 0; %i < strLen(%illegal); %i++) {
		if(strPos(getSubStr(%string, %i, 1), %allowed) == -1) {
			return false;
		}
	}

	return true;
}

function BLG_GDC::verifyNumeric(%this, %num) {
	if(%num $= (%num + 0)) {
		return true;
	} else {
		return false;
	}
}

function BLG_GDC::verifyAlphanumeric(%this, %string) {
	%allowed = "abcdefghijklmnopqrstuvwxyz1234567890";
	for(%i = 0; %i < strLen(%illegal); %i++) {
		if(strPos(getSubStr(%string, %i, 1), %allowed) == -1) {
			return false;
		}
	}
}

function BLG_GDC::finalizeObject(%this, %objId) {
	%obj = %this.SG.objData[%objId];
	if(isObject(%obj)) {
		%newobj = eval("return new " @ %obj.objClass @ "();");
		%newobj.setName(%obj.name);

		for(%i = 0; %i < %obj.attributes; %i++) {
			eval("%newobj." @ %obj.attributeDat[%i] @ "=" @ %obj.attributeVal[%i]@";"); // TODO: remove eval
		}

		%newobj.BLG__OBJID = %objId;
		if(%obj.command !$= "") {
			%newobj.command = %obj.command;
		}

		BLG.debug("Executing Object [" @ %objId @ "]...");
		BLG.debug(%eval, 3);

		%obj.finalized = true;
		%obj.object = %newobj;

		for(%i = 0; %i < %obj.children; %i++) {
			if(!%obj.child[%i].finalized) {
				%obj2 = %this.finalizeObject(%obj.child[%i].id);
			} else {
				%obj2 = %obj.child[%i].object;
			}
			%obj.object.add(%obj2);
		}

		%this.SG.add(%obj.object);
		return %obj.object;
	}
}

function BLG_GDC::setObjectAttribute(%this, %objId, %data, %value) {
	%obj = %this.SG.objData[%objId];
	if(isObject(%obj.object)) {
		eval(%obj.object @ "." @ %data @ " = " @ %value @ ";");
		BLG.debug("Attribute update");
	} else {
		%obj.attributeDat[%obj.attributes] = %data; 
		%obj.attributeVal[%obj.attributes] = %value;
		%obj.attributes++;
	}
}

function BLG_GDC::setObjectChild(%this, %objId, %subId) {
	%obj = %this.SG.objData[%objId];
	%child = %this.SG.objData[%subId];
	if(%child.parent $= "") {
		%obj.child[%obj.children] = %child;
		%child.parent = %obj;
		%obj.children++;
	}
}

function BLG_GDC::initiateObject(%this, %objId, %objClass, %name, %root) {
	%obj = new ScriptObject() {
		class = BLG_GDC_ObjectConstructor;

		id = %objId;
		finalized = false;

		root = %root; //bool - root object (gui, profile)
		name = %name; //string - object name
		objClass = %objClass; //string - object class (GuiWindow, GuiButton, GuiProfile)

		object = "";

		children = 0;
		attributes = 0;
	};

	%this.SG.objs++;
	%this.SG.objData[%objId] = %obj;
	%this.SG.add(%obj);
}

function BLG_GDC::handleMessage(%this, %msg) {
	BLG.debug("[" @ %msg @ "]");
	%funcId = getField(%msg, 0);
	%objId = getField(%msg, 1);
	if(%this.loading) {
		%this.partsReceived++;
		LoadingProgress.setValue(%this.partsReceived/%this.prepareParts);
	}
	if(!%this.verifyNumeric(%objId)) {
		BLG.debug("Invalid ObjId", 0);
		return;
	}

	switch(%funcId) {
		case 0: //New Object
			if(BLG_GDC.SG.objData[%objId] !$= "") {
				//Object already exists, someone broke the server mod
				BLG.debug("GUI Object sent for creation with objId [" @ %objId @ "] is using a already existing objId", 0);
			} else {
				%objClass = getField(%msg, 2);
				%name = getField(%msg, 3);
				%root = getField(%msg, 4);

				if(!%this.verifyAlphabetic(%objClass) || !%this.verifyAlphanumeric(%name)) {
					//Watch out guys, we're dealing with a badass over here
					BLG.debug("Verify String test returned false for objId [" @ %objId @ "]. Could be possible attempt to run malicious script");
					BLG.debug("Message: [" @ %msg @ "]");
				}

				if(%objClass $= "") {
					BLG.debug("Invalid initiate message [" @ %msg @ "] (bad class)", 0);
				} else if(%root != 1 && %root != 0 && %root !$= "") {
					BLG.debug("Invalid initiate message [" @ %msg @ "] (bad root value)", 0);
				}

				if(%root) {
					%this.roots = trim(%this.roots SPC %objId);
				}
				%this.initiateObject(%objId, %objClass, %name, %root);
				%this.setObjectAttribute(%objId, "command", "\"BLG_GDC.commandCallback(" @ %objId @ ");\"");
				%this.setObjectAttribute(%objId, "altCommand", "\"BLG_GDC.altCommandCallback(" @ %objId @ ");\"");
				%this.setObjectAttribute(%objId, "closeCommand", "\"BLG_GDC.closeCommandCallback(" @ %objId @ ");\"");
			}

		case 1: //Set Attribute
			if(BLG_GDC.SG.objData[%objId] $= "") {
				//Object doesnt exist, someone broke the server mod
				BLG.debug("GUI Object attribute change for objId [" @ %objId @ "] is trying to change a non-existant object!", 0);
			} else {
				%data = getField(%msg, 2);
				%value = getField(%msg, 3);

				if(%data $= "") {
					BLG.debug("GUI Object attribute change for objId [" @ %objId @ "] is trying to change a blank attribute!", 0);
					return;
				} else if(!%this.verifyAlphabetic(%data)) {
					BLG.debug("Bad data name", 0);
					return;
				}
				%value = "\"" @ %value @ "\"";
				if(%data $= "command" || %data $= "altcommand" || %data $= "closecommand") {
					BLG.debug("GUI Object id [" @ %objId @ "] tried to set a callback invalidly. Possible attempt to run bad code.", 0);
				}

				%this.setObjectAttribute(%objId, %data, %value);
			}

		case 2: //Set Parent Object (After 0-1 are finished)
			%parentId = getField(%msg, 2);
			if(BLG_GDC.SG.objData[%objId] $= "" || BLG_GDC.SG.objData[%parentId] $= "") {
				BLG.debug("GUI Object id [" @ %objId @ "] and/or [" @ %parentId @ "] is invalid, but tried to parent", 0);
			} else {
				%this.setObjectChild(%objId, %parentId);
			}

		//case 3: //Finished with setting items
		//	for(%i = 0; %i < getWordCount(%this.roots); %i++) {
		//		%this.finalizeObject(getWord(%this.roots, %i));
		//	}

		//All from here are non-registry calls

		//case 4: //Set custom callback
		//	%command = "\"BLG_GDC.handleCallback(" @ %objId @ ");\"";
		//	%obj = BLG_GDC.SG.objData[%objId];
		//	%obj.command = %command;

		case 5: //Open/Close
			%open = getField(%msg, 2);
			%obj = BLG_GDC.SG.objData[%objId];
			if(%open) {
				canvas.pushDialog(%obj.object);
			} else {
				canvas.popDialog(%obj.object);
			}

		case 6: //Get Value
			%obj = BLG_GDC.SG.objData[%objId];
			commandtoserver('BLG_GuiReturn', "1" TAB %objId TAB %obj.object.getValue());

		case 7: //Get Property
			%obj = BLG_GDC.SG.objData[%objId];
			%base = %obj.object;
			%value = getField(%msg, 2);
			if(%this.verifyAlphabetic(%value)) {
				eval("%v = " @ %obj @ "." @ %value @ ";");
				commandtoserver('BLG_GuiReturn', "2" TAB %objId TAB %value TAB %v);
			}

		case 8: //setValue
			
	}
}

function BLG_GDC::commandCallback(%this, %objId) {
	commandtoserver('BLG_GuiReturn', "0" TAB %objId TAB "command");	
}

function BLG_GDC::closeCommandCallback(%this, %objId) {
	commandtoserver('BLG_GuiReturn', "0" TAB %objId TAB "close");	
}

function BLG_GDC::altCommandCallback(%this, %objId) {
	commandtoserver('BLG_GuiReturn', "0" TAB %objId TAB "alt");
}

function clientCmdBLG_ObjectInfo(%msg) {
	BLG_GDC.handleMessage(%msg);
}

function clientCmdBLG_guiTransferFinished() {
	%this = BLG_GDC;
	%this.loading = false;
	for(%i = 0; %i < getWordCount(%this.roots); %i++) {
		%this.finalizeObject(getWord(%this.roots, %i));
	}
}

function clientCmdMissionPreparePhaseBLG(%parts) {
	BLG_GDC.prepareParts = %parts;
	BLG_GDC.partsReceived = 0;
	BLG_GDC.loaded = true;
	LoadingProgressTxt.setValue("Downloading BLG GUI Elements");
	LoadingProgress.setValue(0);
	commandtoserver('MissionPreparePhaseBLGAck');
	echo("*** Starting BLG GUI Download Phase - (" @ %parts @ " parts)");
}

package BLG_GDC_Package {
	function disconnectedCleanup() {
		parent::disconnectedCleanup();
		for(%i = 0; %i < BLG_GDC.SG.getCount(); %i++) {
			%obj = BLG_GDC.SG.getObject(%i);
			canvas.popDialog(%obj);
		}
		BLG_GDC.SG.deleteAll();
		BLG_GDC.SG.delete();
		BLG_GDC.SG = new ScriptGroup();
	}
};

activatePackage(BLG_GDC_Package);