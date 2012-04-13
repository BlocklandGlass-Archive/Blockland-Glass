//================================================
// Title: Glass Keybind Server
//================================================

if(!isObject(BLG_GKS)) {
	new ScriptObject(BLG_GKS){
		binds = 0;
	};
}

function BLG_GKS::newBind(%this, %name, %callback) {
	%this.bind[%this.binds] = %name;
	%this.bindCb[%this.binds] = %callback;
	%this.binds++;
}

function serverCmdBLG_bindCallback(%client, %name) {
	%this = BLG_GKS;
	for(%i = 0; %i < BLG_GKS.binds; %i++) {
		if(%this.bind[%i] $= %name) {
			eval(%this.bindCb[%i] @ "(" @ %client @ ");");
			//don't break, as we may have multiple binds to that name
		}
	}
}

package BLG_GKS_Package {
	function serverCmdMissionPreparePhaseBLGAck(%client) {
		parent::serverCmdMissionPreparePhaseBLGAck(%client);
		for(%i = 0; %i < BLG_GKS.binds; %i++) {
			commandtoclient(%client, 'BLG_requireBind', BLG_GKS.bind[%i]);
		}
	}
};
activatePackage(BLG_GKS_Package);