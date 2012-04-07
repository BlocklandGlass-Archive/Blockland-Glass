//================================================
// Title: Glass RTB server hook 
//================================================

//================================================
// MissionPrepare
//================================================

package BLG_S_MissionPrepare {
	function GameConnection::onGUIDone(%client) {
		if(!%client.hasBLG) {
			parent::onGUIDone(%client);
		} else {
			%client.hasDownloadedGUI = 1;
			commandToClient(%client, 'RTB_receiveComplete');
			commandToClient(%client, 'MissionPreparePhaseBLG', BLG_GDS.getPartCount());
			%client.currentPreparePhase = 2;
		}
	}
};

activatepackage(BLG_S_MissionPrepare);