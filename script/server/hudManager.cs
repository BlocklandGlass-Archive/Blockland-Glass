//================================================
// Title: Glass HUD Server
//================================================

if(!isObject(BLG_HUDS)) {
	new ScriptObject(BLG_HUDS) {
		items = 0;
	};
}

function BLG_HUDS::registerValue(%this, %type, %title, %value) {
	if(%this.itemTitle[%title] $= "") {
		%this.itemNam[%this.items] = %title;
		%this.itemTyp[%this.items] = %type;
		%this.itemVal[%this.items] = %value;
		%this.itemTitle[%title] = %this.items;
		%this.items++;
	}
}

function GameConnection::updateHUD(%client, %title, %value) {
	if(%this.itemTitle[%title] !$= "") {
		commandtoclient(%client, 'BLG_HUD', "1", %title, %value);
	}
}

package BLG_HUDS_Package {
	function serverCmdMissionPreparePhaseBLGAck(%client) {
		parent::serverCmdMissionPreparePhaseBLGAck(%client);
		for(%i = 0; %i < BLG_HUDS.items; %i++) {
			commandtoclient(%client, 'BLG_HUD', "0", BLG_HUDS.itemTyp[%i], BLG_HUDS.itemNam[%i], BLG_HUDS.itemVal[%i]);			
		}
	}
};
activatePackage(BLG_HUDS_Package);

BLG_HUDS.registerValue("text", "AMMO", "None");
BLG_HUDS.registerValue("progressbar", "HEALTH", "1\n%100");