//================================================
// Title: Glass Server Connection
//================================================

if(!isObject(BLG_GSC)) {
	
	%did = $BLG::Connections::DeviceId;
	new ScriptObject(BLG_GSC) {
		host = "api.blockland.zivle.com";
		//host = "localhost";
		port = 9898;
		pubid = $BLG::Server::PubId;
		deviceId = %did;
	};

	if($Server::Dedicated) {
		BLG_GSC.gea = new ScriptObject() { class = GEA; };
		if($BLG::Server::PrivateKey $= "") {
			BLG.debug("New private key: " @ ($BLG::Server::PrivateKey = BLG_GSC.gea.newKey()));
		} else {
			BLG_GSC.gea.setKey($BLG::Server::PrivateKey);
		}

		if(BLG_GSC.pubid $= "") {
			%byte = new ScriptObject() { class = Byte; };
			for(%i = 0; %i < 10; %i++) {
				%byte.setInteger(getRandom(0, 255));
				$BLG::Server::PubId = $BLG::Server::PubId @ %byte.getHex();
			}
			BLG_GSC.pubid = $BLG::Server::PubId;
			%byte.delete();
		}

		export("$BLG::*", "config/BLG/prefs.cs");
	}

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

function BLG_GSC::registerSecureHandle(%this, %key, %func) {
	%this.secureHandle[%key] = %func;
}

function BLG_GSC::registerRelayHandle(%this, %key, %func) {
	%this.relayHandle[%key] = %func;
}

function BLG_GSC::init(%this) {
	if($Pref::Player::NetName $= "") {
		BLG.debug("No name. Aborting connect");
		return;
	}
	%this.tcp.connect(%this.host @ ":" @ %this.port);
}

function BLG_GSC::relay(%this, %target, %msg, %gea) {
	if(%gea $= "") {
		%this.tcp.send("relay" TAB %target TAB strReplace(%msg, "\t", "\\t") @ "\r\n");
	} else {
		%this.tcp.send("relay" TAB %target TAB "~" @ %gea.encrypt("GEA-ENC" TAB %msg) @ "\r\n");
	}
}

//================================================
// Title: GSC TCP
//================================================

function BLG_GSC_TCP::onConnected(%this) {
	BLG.debug("Connected to Glass Server, sending handshake");
	%this.send("handshake" TAB "init" TAB $Pref::Player::NetName TAB ($Server::Dedicated ? "server" : "client") TAB BLG.internalVersion @ ($Server::Port ? "\t" @ $Server::Port : "") @ "\r\n");
}

function BLG_GSC_TCP::onLine(%this, %line) {
	if(%this.reconnect) {
		BLG_GNS.newNotification("Connection Resolved", "Reconnected");
		%this.reconnect = false;
	}

	%this.reconnect = false;
	BLG.debug("Got line > " @ %line);
	%proto = getField(%line, 0);

	if(%proto $= "handshake") {
		switch$(getField(%line, 1)) {
			case "challenge":
				%d = getField(%line, 3)-getSubStr(getDateTime(), 9, 2);
				if(%d < 0) {
					%d += 24;
				}
				
				%this.timeDifference = %d;
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
					%this.authed = true;
					if($Server::Dedicated) {
						%this.send("handshake\tdata\t" @ BLG.internalVersion @ "\t" @ BLG_GSC.pubId @ "\t" @ $Pref::Server::Port @ "\t" @ $Server::Name @ (BLG_GSC.deviceId ? "\t" @ BLG_GSC.deviceId : "") @ "\r\n");
					} else {
						%this.send("handshake\tdata\t" @ BLG.internalVersion @ (BLG_GSC.deviceId ? "\t" @ BLG_GSC.deviceId : "") @ "\r\n");
					}
				}
		}
	}  else if(%proto $= "disconnect") {
		if(getField(%line, 1) $= "maintenance") {
			%this.disconnect();
			BLG.debug("Disconnected from Glass Server");
			%this.reconnect = true;
			%this.rcSchedule = BLG_GSC.schedule(getRandom(1000, 10000), init);
		}
	} else if(%this.authed) {
		if(%proto $= "meta") {
			if(getField(%line, 1) $= "id") {
				$BLG::Connections::DeviceId = BLG_GSC.deviceId = getField(%line, 2);
			}
		} else if(%proto $= "relay") {
			if(getSubStr(getField(%line, 2), 0, 1) $= "~") {
				if($Server::Dedicated) {
					%msg = BLG_GSC.gea.decrypt(getField(%line, 2));
				} else {
					%msg = BLG_GRSC.cid[getField(%line, 1)].gea.decrypt(getField(%line, 2));
				}
				echo(%msg);
				if(strPos(%msg, "GEA-ENC") == 0) {
					%msg = getSubStr(%msg, 8, strLen(%msg));
				} else {
					BLG.error("Bad Crypt");
					return;
				}
				%msg = strReplace(%msg, "\"", "\\\"");
				echo(%this.parent.secureHandle[getField(%msg, 0)] @ "(" @ getField(%line, 1) @ ", \"" @ %msg @ "\");");
				eval(%this.parent.secureHandle[getField(%msg, 0)] @ "(" @ getField(%line, 1) @ ", \"" @ %msg @ "\");");
			} else {
				%msg = strReplace(strReplace(getField(%line, 2), "\"", "\\\""), "\\t", "\t");
				echo(%this.parent.relayHandle[getField(%msg, 0)] @ "(" @ getField(%line, 1) @ ", \"" @ %msg @ "\");");
				eval(%this.parent.relayHandle[getField(%msg, 0)] @ "(" @ getField(%line, 1) @ ", \"" @ %msg @ "\");");
			}
		} else {
			if(%this.parent.handle[%proto] !$= "") {
				eval(%this.parent.handle[%proto] @ "(%line);");
			} else {
				BLG.debug("Unhandled Line: " @ %line);
			}
		}
	}
}

function BLG_GSC_TCP::onConnectFailed(%this) {
	if(!%this.reconnect && !$Server::Dedicated) {
		BLG_GNS.newNotification("Connection Issue", "Failed to connect to BLG Server. Attempting reconnect.", "", 10000);
	}

	%this.reconnect = true;
	BLG.debug("Retrying BLG server connection");
	%this.rcSchedule = BLG_GSC.schedule(getRandom(1000, 10000), init);
}

function BLG_GSC_TCP::onDisconnect(%this) {
	if(!$Server::Dedicated) {
		BLG_GNS.newNotification("Connection Issue", "Unexpected disconnect from BLG Server. Attempting reconnect.");
		BLG.debug("Disconnected from Glass Server");
		%this.rcSchedule = BLG_GSC.schedule(getRandom(1000, 10000), init);
	} else {
		BLG.debug("Disconnected from Glass Server");
		%this.rcSchedule = BLG_GSC.schedule(getRandom(1000, 10000), init);
	}
}

package BLG_GSC_Package {
	function MM_AuthBar::blinkSuccess(%this) {
		BLG_GSC.init();
		parent::blinkSuccess(%this);
	}

	function postServerTCPObj::connect(%this, %addr) {
		parent::connect(%this, %addr);
		if($Server::Dedicated && !BLG_GSC_TCP.authed) {
			BLG_GSC.init();
		}
	}
};