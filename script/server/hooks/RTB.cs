//================================================
// Title: Glass RTB server hook8j
//================================================

//================================================
// MissionPrepare
//================================================

package BLG_MissionPrepare {
	function GameConnection::onGUIDone(%client) {
		if(!%client.hasBLG) {
			parent::onGUIDone(%client);
		}
		else {
			%client.hasDownloadedGUI = 1;
			commandToClient(%client,'RTB_receiveComplete');
			commandToClient(%client, 'MissionPreparePhase3');
			%client.currentPreparePhase = 2;
			BLG_GDS.startTransfer(%client);
		}
	}

	function serverCmdMissionPreparePhase3Ack(%client) {
		BLG_GDS.startTransfer(%client);
	}

	function BLG_GDS::transferFinished(%client) {
		%client.currentPhase = 0;
		%client.BLG_DownloadedGUI = true;
		commandToClient(%client,'MissionStartPhase1', $missionSequence, $Server::MissionFile);
	}
};