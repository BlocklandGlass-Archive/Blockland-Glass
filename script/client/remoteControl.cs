//================================================
// Title: Glass Remote Server Control
//================================================

if(!isObject(BLG_GRSC)) {
	new ScriptGroup(BLG_GRSC);
}

BLG_GSC.registerHandle("rsc", "BLG_GRSC.onLine");

function BLG_GRSC::onLine(%this, %line) {
	switch$(getField(%line, 1)) {
		case "server":
			%cmd = getField(%line, 2);

			if(%cmd $= "clear") {
				%this.clear();
			} else if(%cmd $= "add") {
				new ScriptObject() {
					class = BLG_ServerHandle;

					cid = getField(%line, 3);
					pubId = getField(%line, 4);
				};
			}
	}
}