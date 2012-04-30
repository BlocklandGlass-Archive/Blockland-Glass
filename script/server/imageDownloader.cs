//================================================
// Title: Glass Image Downloader Server
//================================================

if(!isObject(BLG_IDS)) {
	new ScriptObject(BLG_IDS) {
		files = 0;
	};
}

function BLG_IDS::newFont(%this, %url, %fontName) {
	%url = strReplace(%url, "http://", "");
	%host = getSubStr(%url, 0, strPos(%url, "/"));
	%path = getSubStr(%url, strPos(%url, "/"), strLen(%url));

	%this.file[%this.files] = %host TAB %path;
	%this.fileName[%this.files] = %fontName @ ".gft";
	%this.fileType[%this.files] = "font";
	%this.files++;
}

function BLG_IDS::newImage(%this, %url, %imageName) {
	%url = strReplace(%url, "http://", "");
	%host = getSubStr(%url, 0, strPos(%url, "/"));
	%path = getSubStr(%url, strPos(%url, "/"), strLen(%url));

	%this.file[%this.files] = %host TAB %path;
	%this.fileName[%this.files] = %imageName;
	%this.fileType[%this.files] = "image";
	%this.files++;
}

function BLG_IDS::startPhase(%this, %client) {
	%fileCount = %this.files;
	%client.currentPreparePhase = 3;
	commandToClient(%client, 'MissionPreparePhaseBLG2', %fileCount);
}

function BLG_IDS::sendFileData(%this, %client, %file) {
	%file += 0;
	if(%file < %this.files) {
		BLG.debug("Sending file " @ %file);
		commandToClient(%client, 'BLG_IDC_fileData', %this.file[%file], %this.fileType[%file], %this.fileName[%file]);
		%this.schedule(50, sendFileData, %client, %file++);
	} else {
		commandToClient(%client, 'BLG_IDC_finishedFileData');
	}
}

function BLG_IDS::endPhase(%this, %client) {
	%client.currentPhase = 0;
	%client.BLG_DownloadedImages = true;
	commandToClient(%client,'MissionStartPhase1', $missionSequence, $Server::MissionFile);
}

function serverCmdMissionPreparePhaseBLG2Ack(%client) {
	BLG_IDS.sendFileData(%client);
}

function serverCmdMissionPreparePhaseBLG2Complete(%client) {
	BLG_IDS.endPhase(%client);
}

BLG_IDS.newImage("http://blockland.zivle.com/style/img/logo.jpg", "BLG.jpg");