//================================================
// Title: Glass API
//================================================

if(!isObject(GAPI)) {
	new ScriptObject(GAPI) {};
}

function GAPI::newKeybind(%this, %name, %callback) {
	if(BLG.implementation $= "server") {
		return BLG_GKS.newBind(%name, %callback);
	}
}

function GAPI::registerGuiObject(%this, %obj) {
	if(BLG.implementation $= "server") {
		return BLG_GDS.registerObject(%obj);
	}
}

function GAPI::getDataobject(%this, %obj) {
	if(BLG.implementation $= "server") {
		return BLG_GDS.getDataObject(%obj);
	}
}

function GAPI::newHudObject(%this, %type, %title, %value) {
	if(BLG.implementation $= "server") {
		return BLG_HUDS.registerValue(%type, %title, %value);
	}
}

function GAPI::newFont(%this, %url, %fontName) {
	if(BLG.implementation $= "server") {
		return BLG_IDS.newFont(%url, %fontName);
	}
}

function GAPI::newImage(%this, %url, %imageName) {
	if(BLG.implementation $= "server") {
		BLG_IDS.newImage(%url, %imageName);
	}
}