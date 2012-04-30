//Blockland Glass Version 1.0 Client Implementation
exec("./core.cs");

BLG.start("client");


function clientCmdBLG_Handshake(%version, %internalVersion) {
	BLG.debug("Server version id: " @ %version);
	commandToServer('BLG_HandshakeResponse', canvas.getExtent());
}

//Thank-you, dearest Iban
$cArg[8, mFloor($cArgs[8])] = "BLG" TAB BLG.internalVersion TAB BLG.versionId;
$cArgs[8]++;
exec("./Support_ModVersion.cs");