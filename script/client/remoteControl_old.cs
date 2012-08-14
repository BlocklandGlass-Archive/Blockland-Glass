//================================================
// Title: Glass Remote Control
//================================================

$BLG::GRC::UseDefaultProxy = true;

$BLG::GRC::AlternateProxyAddr = "myAlternateProxy.com"; //Paranoid? Change UseDefaultProxy to false and host your own. You'll need to port-forward, btw.
$BLG::GRC::AlternateProxyPort = 9898;

if(!isObject(BLG_GRC)) {
	new ScriptObject(BLG_GRC);
}

if($BLG::GRC::UseDefaultProxy) {
	BLG_GSC.registerHandle("server", "BLG_GSC.onServerLine");
	BLG_GSC.registerHandle("rc", "BLG_GSC.onLine");
}

function BLG_GSC::onLine(%this, %line) {
	%arg1 = getField(%line, 0);
	switch$(%arg1) {
		case "console":
			BLG_RemoteControl.newConsole(%line);

		case "playerCount":
			%players = getField(%line, 1);
			%max = getField(%line, 2);
			if(%players == 0) {
				%text = "\c1";
			} else if(%player == %max) {
				%text = "\c3";
			} else {
				%text = "\c2";
			}
			BLG_RemoteControl_PlayerCount.setValue(%text @ %players @ "/" @ %max);

		case "serverName":
			BLG_RemoteControl_ServerName.setValue("\c3" @ getField(%line, 1));

		case "addPlayer":
			BLG_RemoteControl_PlayerList.addRow(BLG_RemoteControl_PlayerList.getCount(), getField(%line, 3) TAB getField(%line, 1) TAB getField(%line, 2));

		case "removePlayer":
			//TODO

		case "chat":
			BLG_RemoteControl.newChat(getField(%line, 1), getField(%line, 2));
	}
}

function BLG_GSC::onServerLine(%this, %line) {
	//TODO
}