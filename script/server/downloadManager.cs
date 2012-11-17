//================================================
// Title: Glass Server Updater Server
//================================================
//if(!$Server::Dedicated) {
//	return;
//}

if(!isObject(BLG_SUS)) {
	%sus = new ScriptObject(BLG_SUS);
	%tcp = new TCPObject(BLG_SUS_RTBTCP);
	%dl = new TCPObject(BLG_SUS_RTBDL);
	%list = new ScriptGroup(BLG_SUS_Updates);
	%sus.list = %list;
	%sus.RTBTCP = %tcp;
	%sus.RTBDL = %dl;
	%sus.RTBDL.queue = newQueue("BLG_SUS_RTBDL.queue");
}

function BLG_SUS::getUpdates(%this) {
	%fo = new FileObject();

	%file = findFirstFile("Add-Ons/*.zip");
	while(%file !$= "") {
		if(strLen(%file) - strLen(strReplace(%file, "/", "")) != 1) {
			error("Nested add-on \"" @ %file @ "\"");
			%file = findNextFile("Add-Ons/*.zip");
			continue;
		}

		%dir = getSubStr(%file, 0, strPos(%file, ".zip"));
		if(isFile(%dir @ "/rtbInfo.txt")) {
			%fo.openForRead(%dir @ "/rtbInfo.txt");
			while(!%fo.isEOF()) {
				%line = %fo.readLine();
				if(getWord(%line, 0) $= "id:") {
					%id = getWord(%line, 1);
				} else if(getWord(%line, 0) $= "version:") {
					%version = getWord(%line, 1);
					if(%version > 1) {
						%version -= 1;
					}
				}
			}
			%fo.close();
			%query = %query @ %id @ "-" @ %version @ ".";
		}

		%file = findNextFile("Add-Ons/*.zip");
	}

	%fo.delete();

	%query = getSubStr(%query, 0, strLen(%query)-1);
	%this.RTBTCP.rtbList = %query;

	%this.RTBTCP.requestId = 2;
	%this.RTBTCP.connect("api.returntoblockland.com:80");
	echo(%query);
}

function BLG_SUS::updateRTBMod(%this, %rtbid) {
	%tcp = %this.RTBDL;
	%tcp.queue.addItem(%rtbid);
}

//================================================
// Communications
//================================================

BLG_GSC.registerRelayHandle("updates", "BLG_SUS.onLine");

function BLG_SUS::addUpdate(%this, %title, %version, %callback) {
	%update = new ScriptObject() {
		class = "BLG_SUS_Update";
		callback = %callback;

		name = %title;
		version = %verison;
	};
	BLG_SUS.list.add(%update);
}

function BLG_SUS::onLine(%this, %sender, %message, %moar) {
	if(%message $= "updates\tgetupdates") {
		for(%i = 0; %i < %this.list.getCount(); %i++) {
			%obj = %this.list.getObject(%i);
			%str = %str @ "<->" @ %obj.name @  "<.>" @ %obj.version;
		}
	}
	echo(getSubStr(trim(%str), 3, strLen(%str)));
	BLG_GSC.relay(%sender, "updates\t" @ getSubStr(trim(%str), 3, strLen(%str)));
}

//================================================
// RTB Update Info TCP
//================================================

function BLG_SUS_RTBTCP::onConnected(%this) {
	if(%this.requestId == 1) {
		%con = "n=" @ urlEnc($Pref::Player::NetName) @ "&b=" @ getNumKeyId() @ "&c=GETUPDATES&arg1=0&arg2=" @ $RTB::Version @ "&arg3=" @ $Version;
		%message = "POST /apiRouter.php?d=APIUM HTTP/1.1";
		%message = %message NL "Content-Length: " @ strLen(%con);
		%message = %message NL "Content-Type: application/x-www-form-urlencoded";
		%message = %message NL "Host: api.returntoblockland.com";
		//%message = %message NL "User-Agent: BLG/" @ BLG.internalVersion;
		%message = %message NL "User-Agent: RTB/4.0";
		%message = %message NL "Connection: close\n\n";
		%message = %message @ %con;
	} else if(%this.requestId == 2) {
		%con = "n=" @ urlEnc($Pref::Player::NetName) @ "&b=" @ getNumKeyId() @ "&c=GETUPDATES&arg1=" @ %this.rtbList;
		%message = "POST /apiRouter.php?d=APIMS HTTP/1.1";
		%message = %message NL "Content-Length: " @ strLen(%con);
		%message = %message NL "Content-Type: application/x-www-form-urlencoded";
		%message = %message NL "Host: api.returntoblockland.com";
		//%message = %message NL "User-Agent: BLG/" @ BLG.internalVersion;
		%message = %message NL "User-Agent: RTB/4.0";
		%message = %message NL "Connection: close\n\n";
		%message = %message @ %con;
	}

	
	%this.send(%message);
}

function BLG_SUS_RTBTCP::onLine(%this, %line) {
	if(%this.requestId == 2) {
		if(getField(%line, 0) $= "GETUPDATES") {
			%update = new ScriptObject() {
				class = "BLG_SUS_Update";
				rtb = true;

				id = getField(%line, 2);
				name = getField(%line, 4);
				version = getField(%line, 6);
			};
			BLG_SUS.list.add(%update);
		}
	}
	BLG.debug("RTBTCP: " @ %line);
}

//================================================
// RTB File Downloading TCP
//================================================

function BLG_SUS_RTBDL::queue(%this, %id, %key) {
	%this.curItem = %key;
	%this.connect("forum.returntoblockland.com:80");
}

function BLG_SUS_RTBDL::onConnected(%this) {
	%this.send("GET /dlm/getFile.php?id=" @ %this.curItem @ " HTTP/1.1\nHost: forum.returntoblockland.com");
}

function BLG_SUS_RTBDL::onLine(%this, %line) {

}



//asdf

BLG_SUS.getUpdates();