//Blockland Glass Version 1.0 Server Implementation
exec("./core.cs");

BLG.start("server");

package BLG_Server_Package {
	function GameConnection::onConnectRequest(%client, %ip, %lan, %net, %prefix, %suffix, %arg5, %rtb, %arg7, %arg8, %arg9, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15) {
		%client.hasBLG = false;
		for(%a = 0; %a < getLineCount(%arg[8]); %a++) {
			%line = getLine(%arg[8], %a);
			if(getField(%line, 0) $= "BLG") {
				%client.hasBLG = true;
				%client.BLGVersion = getField(%line, 1);
				break;
			}
		}
		
		if(!%client.hasBLG && BLG.required) {
			%client.delete("You must have Blockland Glass to join this server!<br><br><a:blocklandglass.com/download.php>Download</a>");
		}

		return parent::onConnectRequest(%client, %ip, %lan, %net, %prefix, %suffix, %arg5, %rtb, %arg7, %arg8, %arg9, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15);
	}
};
activatePackage(BLG_Server_Package);