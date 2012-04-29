//================================================
// Title: Glass Image Downloader Server
//================================================

if(!isObject(BLG_IDS)) {
	new ScriptObject(BLG_IDS) {
		files = 0;
	};
}

function BLG_IDS::newFont(%this, %url, %fontName) {
	%host = getSubStr(%url, 0, strPos("/"));
	%host = strReplace(%url, "http://", "");
	%path = getSubStr(%url, strPos("/"), strLen(%url));

	%this.file[%this.files] = %host TAB %path;
	%this.fileName[%this.files] = %fontName @ ".gft";
	%this.fileType[%this.files] = "font";
	%this.files++;
}

function BLG_IDS::newImage(%this, %url, %imageName) {
	%host = getSubStr(%url, 0, strPos("/"));
	%host = strReplace(%url, "http://", "");
	%path = getSubStr(%url, strPos("/"), strLen(%url));

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
		commandToClient(%client, 'BLG_IDC_fileData', %this.file[%file], %this.fileType[%file], %this.fileName[%file]);
		%this.schedule(50, sendFileData, %client, %file++);
	}
}

function BLG_IDS::endPhase(%this, %client) {

}