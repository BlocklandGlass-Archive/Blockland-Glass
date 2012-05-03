//Blockland Glass Version 1.0 Client Implementation
exec("./core.cs");

BLG.start("client");


function clientCmdBLG_Handshake(%version, %internalVersion) {
	BLG.debug("Server version id: " @ %version);
	commandToServer('BLG_HandshakeResponse', canvas.getExtent());
}

package BLG_Client {
	function onExit() {
		%r = "config/BLG/client/cache/*";
		%file = findFirstFile(%r);
		while(%file !$= "") {
			fileDelete(%file);
			%file = findNextFile(%r);
		}
		parent::onExit();
	}
};
activatePackage(BLG_Client);

//Thank-you, dearest Iban
$cArg[8, mFloor($cArgs[8])] = "BLG" TAB BLG.internalVersion TAB BLG.versionId;
$cArgs[8]++;
exec("./Support_ModVersion.cs");