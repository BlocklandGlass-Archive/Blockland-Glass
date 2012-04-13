//================================================
// Title: Glass clean server hook 
//================================================

//================================================
// MissionPrepare
//================================================

package BLG_S_MissionPrepare {
	function GameConnection::MissionPhase1(%client) {
		if(!%client.hasBLG) {
			parent::onGUIDone(%client);
		} else {
			%client.hasDownloadedGUI = 1;
			commandToClient(%client, 'RTB_receiveComplete');
			commandToClient(%client, 'MissionPreparePhaseBLG', BLG_GDS.getPartCount());
			%client.currentPreparePhase = 2;
		}
	}

	function GameConnection::loadMission(%this) { //Yep, cleanly ripped from RTB
		if(%this.isAIControlled()) {
			Parent::loadMission(%this);
		} else {
			if(%this.hasBLG) {
				%this.currentPhase = -1;
				
				%this.currentPreparePhase = 2;
				commandToClient(%client, 'MissionPreparePhaseBLG', BLG_GDS.getPartCount());

				echo("*** Sending mission load to client: " @ $Server::MissionFile);
			}
			else
				Parent::loadMission(%this);
		}
	}
};

activatepackage(BLG_S_MissionPrepare);