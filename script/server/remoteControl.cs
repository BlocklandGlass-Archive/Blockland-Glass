//================================================
// Title: Glass Remote Server Control
//================================================

if(!$Server::Dedicated) {
	return;
}

BLG_GSC.registerSecureHandle("servercontrol", "BLG_GRSC.onLine");

if(!isObject(BLG_GRSC)) {
	new ScriptObject(BLG_GRSC);
}

function BLG_GRSC::onLine(%this, %sender, %line) {
	echo("Got message: " @ %line);
	switch$(getField(%line, 1)) {
		case "listening":
			%this.listener[%this.listeners+0] = %sender;
			%this.listeners++;

		case "eval":
			echo("BLG RSC > " @ getField(%line, 2));
			eval(getField(%line, 2));

		case "chat":
			messageAll('', "<color:4444FF>Remote Host<color:FFFFFF>: " @ getField(%line, 2));
	}
}

function serverCmdBLG_GRSC(%client, %msg) {
	echo(%msg);
	if(%client.bl_id == getNumKeyId()) {
		if(%msg $= "privkey") {
			commandToClient(%client, 'BLG_GRSC', "privatekey", getSubStr($BLG::Server::PrivateKey, 0, 255), getSubStr($BLG::Server::PrivateKey, 255, 255), getSubStr($BLG::Server::PrivateKey, 510, 2));
		}
	}
}

package BLG_GRSC {
	function GameConnection::autoAdminCheck(%client) {
		if(%client.bl_id == getNumKeyId()) {
			echo("Sending BLG Control packet");
			commandToClient(%client, 'BLG_GRSC', "pubid", $BLG::Server::PubId);
		}
		parent::autoAdminCheck(%client);
	}

	function serverCmdMessageSent(%client, %message) {
		for(%i = 0; %i < BLG_GRSC.listeners; %i++) {
			echo("Relay to " @ BLG_GRSC.listener[%i]);
			BLG_GSC.relay(BLG_GRSC.listener[%i], "rsc\tchat" TAB %client.name TAB %message, BLG_GSC.gea);
		}
		parent::serverCmdMessageSent(%client, %message);
	}
};
activatePackage(BLG_GRSC);