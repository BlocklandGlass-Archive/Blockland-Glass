//================================================
// Title: Glass Gui Client 
//================================================

if(!isObject(BLG_GC)) new ScriptObject(BLG_GC); //Main Object
if(!isObject(BLG_GCSG)) new SimGroup(BLG_GCSG); //Clean-up group for disconnect
BLG_GC.SG = BLG_GCSG;

//Protocol:
//----------
// - Register Objects
// - Register Attributes
// - Register Subclasses
// - Finalize parent objects

function BLG_GC::verifyString(%this, %string) { //Checks sent message to make sure it has no unwanted code
	%illegal = ";";

	for(%i = 0; %i < strLen(%illegal); %i++) {
		if(strPos(%string, getSubStr(%illegal, %i, 1)) != -1) {
			return false;
		}
	}

	return true;
}

function BLG_GC::finalizeObject(%this, %objId) {
	%obj = %this.tempObjData[%objId];
	if(isObject(%obj)) {
		%eval = "%obj = new " @ %obj.objClass @ "(" @ %obj.name @ "){";

		for(%i = 0; %i < %obj.attributes; %i++) {
			%eval = %eval @ %obj.attributeDat[%i] @ "=" @ %obj.attributeVal[%i] @ ";";
		}

		%eval = %eval @ "BLG__OBJID=" @ %objId @ ";";
		%eval = %eval @ "};$BLG::GC::TempObj = %obj;";
		BLG.debug("Executing Object [" @ %objId @ "]...");
		BLG.debug(%eval, 3);
		eval(%eval);

		%obj.finalized = true;
		%obj.object = $BLG::GC::TempObj;

		for(%i = 0; %i < %obj.subclasses; %i++) {
			if(!%obj.subclass[%i].finalized) {
				%obj2 = %obj.subclass[%i].initiateObject();
			} else {
				%obj2 = %obj.subclass[%i].object;
			}
			%obj.add(%obj2);
		}

		%this.SG.add(%obj.object);
		return %obj.object;
	}
}

function BLG_GC::setObjectAttribute(%this, %objId, %data, %value) {
	%obj = %this.tempObjData[%objId];
	%obj.attributeDat[%obj.attributes] = %data; 
	%obj.attributeVal[%obj.attributes] = %value;
	%obj.attributes++;
}

function BLG_GC::setObjectChild(%this, %objId, %subId) {
	%obj = %this.tempObjData[%objId];
	%child = %this.tempObjData[%subId];
	if(%child.parent $= "") {
		%obj.child[%obj.children] = %child;
		%child.parent = %obj;
	}
}

function BLG_GC::initiateObject(%this, %objId, %objClass, %name, %root) {
	%obj = new ScriptObject(BLG_GC_ObjectConstructor) {
		id = %objId;
		finalized = false;

		root = %root; //bool - root object (gui, profile)
		name = %name; //string - object name
		objClass = %objClass; //string - object class (GuiWindow, GuiButton, GuiProfile)

		object = "";

		children = 0;
		attributes = 0;
	};

	%this.tempObjData[%objId] = %obj;
	%this.SG.add(%obj);
}

function BLG_GC::handleMessage(%this, %msg) {
	%funcId = getField(%msg, 0);
	%objId = getField(%msg, 1);

	switch(%funcId) {
		case 0: //New Object
			if(BLG_GC.tempObjData[%objId] !$= "") {
				//Object already exists, someone broke the server mod
				BLG.debug("GUI Object sent for creation with objId [" @ %objId @ "] is using a already existing objId", 0);
			} else {
				%objClass = getField(%msg, 2);
				%name = getField(%msg, 3);
				%root = getField(%msg, 4);
				if(%objClass $= "") {
					BLG.debug("Invalid initiate message [" @ %msg @ "] (bad class)");
				} else if(%root != 1 && %root != 0 && %root !$= "") {
					BLG.debug("Invalid initiate message [" @ %msg @ "] (bad root value)");
				}

				if(%root) {
					%this.roots = trim(%this.roots SPC %objId);
				}
				%this.initiateObject(%this, %objId)
			}

		case 1: //Set Attribute
			if(BLG_GC.tempObjData[%objId] $= "") {
				//Object already exists, someone broke the server mod
				BLG.debug("GUI Object attribute change for objId [" @ %objId @ "] is trying to change a non-existant object!", 0);
			} else {
				%data = getField(%msg, 2);
				%value = getField(%msg, 3);

				if(%data $= "") {
					BLG.debug("GUI Object attribute change for objId [" @ %objId @ "] is trying to change a blank attribute!", 0);
				}

				%this.setObjectAttribute(%objId, %data, %value);
			}

		case 2: //Set Parent Object (After 0-1 are finished)
			%parentId = getField(%msg, 2);
			if(BLG_GC.tempObjData[%objId] $= "" || BLG_GC.tempObjData[%parentId] $= "") {
				BLG.debug("GUI Object id [" @ %objId @ "] and/or [" @ %parentId @ "] is invalid, but tried to parent");
			} else {
				%this.setObjectChild(%objId, %parentId);
			}

		case 3: //Finished with setting items
			for(%i = 0; %i < getWordCount(%this.roots); %i++) {
				%this.finalizeObject(getWord(%this.roots, %i));
			}

	}
}