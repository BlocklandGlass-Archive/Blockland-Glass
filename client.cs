//Blockland Glass Version 1.0 Client Implementation
exec("./core.cs");

BLG.start("client");

//Thank-you, dearest Iban

$cArg[8, mFloor($cArgs[8])] = "BLG" TAB BLG.internalVersion;
$cArgs[8]++;

if(isPackage(Support_ModVersion_Client))
	return;

package Support_ModVersion_Client
{
	function getConnectArg(%arg) {
		%connectString = "";
		
		for(%i = 0; %i < $cArgs[%arg]; %i++) {
			if(%i) {
				%connectString = %connectString NL $cArg[%arg, %i];
			} else {
				%connectString = $cArg[%arg, %i];
			}		
		}
		return %connectString;
	}

	function GameConnection::setConnectArgs(%client, %lan, %net, %prefix, %suffix, %arg5, %rtb, %arg7, %arg8, %arg9, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15) {
		%arg[8] = getConnectArg(8);
		parent::setConnectArgs(%client, %lan, %net, %prefix, %suffix, %arg5, %rtb, %arg7, %arg8, %arg9, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15);
	}
};

if(!$PackageToss) {
	$PackageToss = true;
	
	if(isPackage(DRPG_Client))
		deactivatePackage(DRPG_Client);

	if(isPackage(DRPG_Client))
		activatePackage(DRPG_Client);
}