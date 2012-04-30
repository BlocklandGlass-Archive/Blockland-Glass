//Blockland Glass Version 1.0 Server Implementation
exec("./core.cs");

BLG.start("server");

function serverCmdBLGHandshakeResponse(%client, %extent) {
	BLG.debug("Handshake response from " @ %client @ ", extent [" @ %extent @ "]");
	%client.BLG_extent = %extent;
}

function GameConnection::getExtent(%client) {
	return %client.BLG_extent;
}

package BLG_Server_Package {
	function GameConnection::onConnectRequest(%client, %ip, %lan, %net, %prefix, %suffix, %arg5, %rtb, %arg7, %arg8, %arg9, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15) {
		%client.hasBLG = false;
		for(%a = 0; %a < getLineCount(%arg[8]); %a++) {
			%line = getLine(%arg[8], %a);
			if(getField(%line, 0) $= "BLG") {
				%client.hasBLG = true;
				%client.BLGVersion = getField(%line, 1);
				%client.BLGVersionId = getField(%line, 2);
				if(%client.BLGVersionId < 2 && BLG.required) {
					%client.delete("You must be running Blockland Glass 1.2 or later to join this server.");
					break;
				}
				break;
			}
		}
		
		if(!%client.hasBLG && BLG.required) {
			%client.delete("You must have Blockland Glass to join this server!<br><br><a:blocklandglass.com/download.php>Download</a>");
		}

		return parent::onConnectRequest(%client, %ip, %lan, %net, %prefix, %suffix, %arg5, %rtb, %arg7, %arg8, %arg9, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15);
	}

	function GameConnection::autoAdminCheck(%client) {
		parent::autoAdminCheck(%client);
		commandToClient(%client, 'BLG_Handshake', BLG.versionId, BLG.internalVersion);
	}
};
activatePackage(BLG_Server_Package);