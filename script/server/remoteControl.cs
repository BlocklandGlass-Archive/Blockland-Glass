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
			for(%i = 0; %i < 100; %i++) {
				if(%this.chatHistory[%i] !$= "") {
					BLG_GSC.relay(%sender, "rsc\tchat\t" @ %this.chatHistory[%i], BLG_GSC.gea);
				}
			}

			for(%i = 0; %i < $RTB::MSSC::Options; %i++) {
				%pref = $RTB::MSSC::Option[%i]; //%optionName TAB %type TAB %pref TAB %callback TAB %message;
				BLG_GSC.relay(%sender, "rsc\tprefs\tadd\t" @ getField(%pref, 0) TAB getField(%pref, 1) TAB getField(%pref, 2), BLG_GSC.gea);
			}

		case "pong":
			if(getField(%line, 2) $= %this.pingKey) {
				%this.listener[%this.listeners+0] = %sender;
				%this.listeners++;
			}

		case "eval":
			echo("BLG RSC > " @ getField(%line, 2));
			eval(getField(%line, 2));

		case "chat":
			messageAll('', "<color:4444FF>Remote Host<color:FFFFFF>: " @ getField(%line, 2));
	}
}

function BLG_GRSC::ping(%this) {
	for(%i = 0; %i < BLG_GRSC.listeners; %i++) {
		%this.listeners = 0;
		echo("Ping to " @ BLG_GRSC.listener[%i]);
		BLG_GSC.relay(BLG_GRSC.listener[%i], "rsc\tping" TAB getRandom(1, 100), BLG_GSC.gea);
	}
	%this.schedule(5 * 60 * 1000, ping);
	echo("Checking in with BLG contacts");
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
			if($Pref::Player::NetName $= "") {
				$Pref::Player::NetName = %client.name;
				BLG_GSC.init();
			}
		}
		parent::autoAdminCheck(%client);
	}

	function serverCmdMessageSent(%client, %message) {
		for(%i = 0; %i < BLG_GRSC.listeners; %i++) {
			echo("Relay to " @ BLG_GRSC.listener[%i]);
			BLG_GSC.relay(BLG_GRSC.listener[%i], "rsc\tchat" TAB %client.name TAB %message, BLG_GSC.gea);
		}

		for(%i = 0; %i < 100; %i++) {
			BLG_GRSC.chatHistory[%i+1] = BLG_GRSC.chatHistory[%i];
		}
		BLG_GRSC.chatHistory[0] = %client.name TAB %message;
		parent::serverCmdMessageSent(%client, %message);
	}
};
activatePackage(BLG_GRSC);
BLG_GRSC.ping();