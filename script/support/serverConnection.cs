//================================================
// Title: Glass Server Connection
//================================================

if(!isObject(BLG_GSC)) {
	new ScriptObject(BLG_GSC) {
		host = "blockland.zivle.com";
		port = 9898;
	};

	if(!isObject(BLG_GSC_TCP)) {
		BLG_GSC.tcp = new TCPObject(BLG_GSC_TCP);
	}
	BLG_GSC.tcp.parent = BLG_GSC;
}

function BLG_GSC::registerHandle(%this, %key, %func) {
	%this.handle[%key] = %func;
}

//================================================
// Title: GSC TCP
//================================================

function BLG_GSC_TCP::onConnected(%this) {
	%this.send("handshake" TAB "init" TAB $Pref::Player::NetName TAB BLG.internalVersion);
}

function BLG_GSC_TCP::onLine(%this, %line) {
	%proto = getField(%line, 0);
	if(%this.parent.handle[%proto] !$= "") {
		eval(%proto @ "(" @ %line @ ");");
	} else {
		BLG.debug("Unhandled Line: " @ %line);
	}
}