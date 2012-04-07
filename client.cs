//Blockland Glass Version 1.0 Client Implementation
exec("./core.cs");

BLG.start("client");


//Thank-you, dearest Iban
$cArg[8, mFloor($cArgs[8])] = "BLG" TAB BLG.internalVersion;
$cArgs[8]++;
exec("./Support_ModVersion.cs");