//================================================
// Title: Glass Remote Control
//================================================

$BLG::GRC::UseDefaultProxy = true;

$BLG::GRC::AlternateProxyAddr = "myAlternateProxy.com"; //Paranoid? Change UseDefaultProxy to false and host your own. You'll need to port-forward, btw.
$BLG::GRC::AlternateProxyPort = 9898;

if(!isObject(BLG_GRC)) {
	new ScriptObject(BLG_GRC);
}