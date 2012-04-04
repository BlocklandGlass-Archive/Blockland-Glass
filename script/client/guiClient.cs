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

function BLG_GC::finalizeObject(%this, %objId) {
	%obj = %this.tempObjData[%objId];
	if(isObject(%obj)) {
		%eval = "new " @ %obj.objClass @ "(" @ %obj.name @ "){";

		for(%i = 0; %i < %obj.attributes; %i++) {
			%eval = %eval @ %obj.attributeDat[%i] @ "=" @ %obj.attributeVal[%i] @ ";";
		}

		for(%i = 0; %i < %obj.subclasses; %i++) {
			%eval = %eval @ %obj.subclass[%i].initiateObject();
		}

		%eval = %eval @ "BLG__OBJID=" @ %objId @ ";";
		%eval = %eval @ "};";
		%this.eval = %eval;
		%this.finalized = true;

		return %this.eval;
	}
}

function BLG_GC::loadObject(%this, %objId) {
	%obj = %this.tempObjData[%objId];
	if(isObject(%obj)) {
		if(%obj.root) {
			BLG.debug("Executing Object [" @ %objId @ "]...");
			BLG.debug(%eval, 3);
			exec(%obj.eval);
		}
	}
}

function BLG_GC::setObjectAttribute(%this, %objId, %data, %value) {
	%obj = %this.tempObjData[%objId];
	%obj.attributeDat[%obj.attributes] = %data; 
	%obj.attributeVal[%obj.attributes] = %value;
	%obj.attributes++;
}

function BLG_GC::setObjectSubobject(%this, %objId, %subId) {
	%obj = %this.tempObjData[%objId];
}

function BLG_GC::initiateObject(%this, %objId, %objClass, %name, %root) {
	%obj = new ScriptObject(BLG_GC_ObjectConstructor) {
		id = %objId;
		finalized = false;

		root = %root; //bool - root object (gui, profile)
		name = %name; //string - object name
		objClass = %objClass; //string - object class (GuiWindow, GuiButton, GuiProfile)
	};

	%this.tempObjData[%objId] = %obj;
}