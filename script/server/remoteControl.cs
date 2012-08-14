//================================================
// Title: Glass Remote Server Control
//================================================

if(!$Server::Dedicated) {
	return;
}

BLG_GSC.registerSecureHandle("servercontrol", "BLG_GRSC.onLine");

if(!isObject(BLG_GRSC)) {
	new ScriptObject(BLG_GRSC);
}

function BLG_GRSC::onLine(%this, %sender, %line) {
	echo("Got message: " @ %line);
	if(getField(%line, 1) $= "eval") {
		echo(getField(%line, 2));
		eval(getField(%line, 2));
	}
}