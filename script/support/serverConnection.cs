//================================================
// Title: Glass Server Connection
//================================================

if(!isObject(BLG_GSC)) {
	new ScriptObject(BLG_GSC) {
		host = "localhost";
		port = 9898;
	};

	if(!isObject(BLG_GSC_TCP)) {
		BLG_GSC.tcp = new TCPObject(BLG_GSC_TCP) {
			authed = false;
		};
	}
	BLG_GSC.tcp.parent = BLG_GSC;
}

function BLG_GSC::registerHandle(%this, %key, %func) {
	%this.handle[%key] = %func;
}

function BLG_GSC::init(%this) {
	%this.tcp.connect(%this.host @ ":" @ %this.port);
}

//================================================
// Title: GSC TCP
//================================================

function BLG_GSC_TCP::onConnected(%this) {
	BLG.debug("Connected to Glass Server, sending handshake");
	%this.send("handshake" TAB "init" TAB $Pref::Player::NetName TAB BLG.internalVersion @ "\r\n");
}

function BLG_GSC_TCP::onLine(%this, %line) {
	BLG.debug("Got line > " @ %line);
	%proto = getField(%line, 0);

	if(%this.authed) {
		if(%this.parent.handle[%proto] !$= "") {
			eval(%proto @ "(" @ %line @ ");");
		} else {
			BLG.debug("Unhandled Line: " @ %line);
		}
	} else if(%proto $= "handshake") {
		switch$(getField(%line, 1)) {
			case "challenge":
				%this.challenge = getField(%line, 2);
				%this.send("handshake\tresponse\t" @ %this.challenge @ "\r\n");

			case "result":
				%result = getField(%line, 2);
				if(%result $= "-1") {
					BLG.error("Auth failed. You are you, right?");
				} else if(%result $= "0") {
					BLG.error("Challenge/Response failed. IP Spoofing?");
				} else if(%result $= "1") {
					echo("BLG Auth Success");
				}
		}
	}
}

function BLG_GSC_TCP::onDisconnect(%this) {
	BLG.debug("Disconnected from Glass Server");
}

package BLG_GSC_Package {
	function MM_AuthBar::blinkSuccess(%this) {
		parent::blinkSuccess(%this);
		BLG_GSC.init();
	}
};
activatePackage(BLG_GSC_Package);