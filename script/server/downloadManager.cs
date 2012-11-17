//================================================
// Title: Glass Server Updater Server
//================================================
//if(!$Server::Dedicated) {
//	return;
//}

if(!isObject(BLG_SUS)) {
	%sus = new ScriptObject(BLG_SUS);
	%tcp = new TCPObject(BLG_SUS_RTBTCP);
	%dl = new ScriptObject(BLG_SUS_RTBDL);
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
}

function BLG_SUS::checkRTBUpdate(%this) {
	%this.RTBTCP.requestId = 1;
	%this.RTBTCP.connect("api.returntoblockland.com:80");
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
	echo("UPDATE!!!");
	%update = new ScriptObject() {
		class = "BLG_SUS_Update";
		callback = %callback;

		name = %title;
		version = %version;
		type = 1;
	};
	%this.list.add(%update);
}

function BLG_SUS::onLine(%this, %sender, %message) {
	if(%message $= "updates\tgetupdates") {
		echo(%this.list.getCount() SPC " update(s) available");
		if(%this.list.getCount() == 0) return;
		for(%i = 0; %i < %this.list.getCount(); %i++) {
			%obj = %this.list.getObject(%i);
			if(%obj.rtb) {
				%type = 0;
			} else if(%obj.blg) {
				%type = 1;
			} else {
				%type = 2;
			}
			%str = %str @ "<->" @ %obj.name @  "<.>" @ %obj.version @ "<.>" @ %type;
		}
		BLG_GSC.relay(%sender, "updates\t" @ getSubStr(trim(%str), 3, strLen(%str)));
	} else if(%message $= "updates\tcheckupdates") {
		echo(%this.list.getCount() SPC " update(s) available");
		if(%this.list.getCount() > 0) {
			BLG_GSC.relay(%sender, "notification\tupdates");
		}
	} else if(getField(%message, 1) $= "confirm") {
		for(%i = 0; %i < %this.list.getCount(); %i++) {
			%obj = %this.list.getObject(%i);
			if(%obj.name $= getField(%message, 2)) {
				if(%obj.rtb) {
					%this.updateRTBMod(%obj.id);
				} else {
					eval(%obj.callback @ "();");
				}
				%this.list.remove(%obj);
				break;
			}
		}
	}
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
		BLG.debug(%line);
		if(getField(%line, 0) $= "GETUPDATES") {
			if(getField(%line, 1) == 0) return;

			%update = new ScriptObject() {
				class = "BLG_SUS_Update";
				rtb = true;

				id = getField(%line, 2);
				name = getField(%line, 4);
				version = "v" @ -getField(%line, 6);
			};
			BLG_SUS.list.add(%update);
		}
	} else if(%this.requestId == 1) {
		//TODO
	}
}

//================================================
// RTB File Downloading TCP
//================================================

function BLG_SUS_RTBDL::queue(%this, %id, %key) {
	%tcp = new TCPObject(BLG_SUS_RTBDL_Downloader) {
		curItem = %key;
		id = %id;
		size = "";
		isFile = false;
		filename = "";
	};
	%tcp.connect("forum.returntoblockland.com:80");
}

function BLG_SUS_RTBDL_Downloader::onConnected(%this) {
	BLG.debug("Connected");
	%this.send("GET /dlm/getFile.php?id=" @ %this.curItem @ " HTTP/1.1\nHost: forum.returntoblockland.com\n\n");
}

function BLG_SUS_RTBDL_Downloader::onLine(%this, %line) {
	if(getWord(%line, 0) $= "Content-Length:")
		%this.size = getWord(%line, 1);

	if(strPos(%line, "Content-Disposition: attachment; filename=") == 0) {
		%this.filename = getSubStr(%line, strLen("Content-Disposition: attachment; filename=\""), strLen(%line)-strLen("Content-Disposition: attachment; filename=")-3);
		echo(%this.filename);
	}

	if(%line $= "Content-Type: application/zip")
		%this.isFile = true;

	if(%this.isFile && %this.size !$= "" && %this.filename !$= "" && %line $= "") {
		%this.setBinarySize(%this.size);
	}
}

function BLG_SUS_RTBDL_Downloader::onBinChunk(%this, %chunk) {
	if(%chunk < %this.size) {
		echo("RTB Download: " @ mFloor((%chunk/%this.size)*100) @ "%");
	} else {
		if(isWriteableFilename("Add-Ons/" @ %this.filename) && isWriteableFilename("config/BLG/updater/temp.zip")) {
			%this.saveBufferToFile("config/BLG/updater/temp.zip"); //Just incase we crash for some reason, we dont leave some sort of corrupted shit
			
			fileCopy("config/BLG/updater/temp.zip", "Add-Ons/" @ %this.filename);
			fileDelete("config/BLG/updater/temp.zip");
			%this.disconnect();
			echo("Done");
			BLG.debug(BLG_SUS_RTBDL.queue.reportFinished(%this.id));
		} else {
			BLG.error("READ ONLY");
			%this.disconnect();
			BLG.debug(BLG_SUS_RTBDL.queue.reportFinished(%this.id));
		}
	}
}

function BLG_SUS_RTBDL_Downloader::onConnectFailed(%this) {
	BLG.error("UNABLE TO CONNECT TO RTB FOR DOWNLOAD");
	BLG_SUS_RTBDL.queue.reportFinished(%this.curItem);
}


//asdf

BLG_SUS.getUpdates();