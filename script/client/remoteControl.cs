//================================================
// Title: Glass Remote Server Control
//================================================

if(!isObject(BLG_GRSC)) {
	new ScriptGroup(BLG_GRSC);
}

BLG_GSC.registerHandle("rsc", "BLG_GRSC.onLine");
BLG_GSC.registerSecureHandle("rsc", "BLG_GRSC.onSecureLine");

function BLG_ServerHandle::setKey(%this, %key) {
	%this.privateKey = %key;
	%gea = new ScriptObject() { class = GEA; };
	%gea.setKey(%key);
	%this.gea = %gea;
}

function BLG_GRSC::saveKeys(%this) {
	%fo = new FileObject();
	%fo.openForWrite("config/BLG/client/keys.dat");
	for(%i = 0; %i < BLG_GRSC.getCount(); %i++) {
		%so = BLG_GRSC.getObject(%i);
		if(%so.privatekey !$= "") {
			%fo.writeLine(%so.pubid TAB %so.privatekey);
		}
	}
	%fo.close();
	%fo.delete();
}

function BLG_GRSC::loadKeys(%this) {
	%fo = new FileObject();
	%fo.openForRead("config/BLG/client/keys.dat");
	while(!%fo.isEOF()) {
		%line = %fo.readLine();
		%this.privCache[getField(%line, 0)] = getField(%line, 1);
	}
	%fo.close();
	%fo.delete();
}

function BLG_GRSC::onSecureLine(%this, %sender, %line) {
	switch$(getField(%line, 1)) {
		case "chat":
			%gui = %this.cid[%sender].gui;
			if(isObject(%gui)) {
				%gui.newChat(getField(%line, 2), getField(%line, 3));
			}
	}
}

function BLG_GRSC::onLine(%this, %line) {
	switch$(getField(%line, 1)) {
		case "server":
			%cmd = getField(%line, 2);

			if(%cmd $= "clear") {
				%this.clear();
			} else if(%cmd $= "add") {
				if(isObject(%this.cid[getField(%line, 3)])) {
					return;
				}

				%so = new ScriptObject() {
					class = BLG_ServerHandle;

					cid = getField(%line, 3);
					pubId = getField(%line, 4);

					name = "";
				};
				%this.add(%so);
				%this.cid[getField(%line, 3)] = %so;
				%so.setKey(%this.privCache[getField(%line, 4)]);
			} else if(%cmd $= "remove") {
				%so = %this.cid[getField(%line, 3)];
				%this.remove(%so);
				%so.delete();
			} else if(%cmd $= "name") {
				if(isObject(%this.cid[getField(%line, 3)])) {
					%this.cid[getField(%line, 3)].name = getField(%line, 4);
				}
			}

			BLG_SelectServer_Scroll.deleteAll();

			for(%i = 0; %i < %this.getCount(); %i++) {
				%gui = new GuiSwatchCtrl() {
					profile = "BLG_TextProfile";
					horizSizing = "right";
					vertSizing = "bottom";
					position = 2 SPC (42*%i)+2;
					extent = "321 40";
					minExtent = "8 2";
					enabled = "1";
					visible = "1";
					clipToParent = "1";
					color = "200 200 200 255";

					new GuiTextCtrl() {
						profile = "BLG_TextCenterProfile";
						horizSizing = "center";
						vertSizing = "center";
						position = "0 11";
						extent = "321 18";
						minExtent = "321 18";
						enabled = "1";
						visible = "1";
						clipToParent = "1";
						text = %this.getObject(%i).name @ " (CID: " @ %this.getObject(%i).cid @ ")";
						maxLength = "255";
					};
					new GuiMouseEventCtrl() {
						profile = "GuiDefaultProfile";
						horizSizing = "right";
						vertSizing = "bottom";
						position = "0 0";
						extent = "321 40";
						minExtent = "8 2";
						enabled = "1";
						visible = "1";
						clipToParent = "1";
						lockMouse = "0";
						BLGSH = true;
						id = %i;
					};
				};
				BLG_SelectServer_Scroll.add(%gui);
			}
	}
}

function clientCmdBLG_GRSC(%type, %msg, %m1, %m2) {
	echo(%msg);
	// If you're getting any of these, the server (thinks that it) belongs to you
	switch$(%type) {
		case "pubid":
			for(%i = 0; %i < BLG_GRSC.getCount(); %i++) {
				if(%msg $= BLG_GRSC.getObject(%i).pubId) {
					BLG_GRSC.currentConnection = BLG_GRSC.getObject(%i);
					BLG.debug("Connected to " @ %msg);
					if(BLG_GRSC.currentConnection.privatekey $= "") {
						commandToServer('BLG_GRSC', "privkey");
					}
					return;
				}
			}
			BLG.error("Connected to server claiming to be yours, but is unlisted. wtf?");

		case "privatekey":
			BLG_GRSC.currentConnection.setKey(%msg @ %m1 @ %m2);
			echo("PrivateKey is " @ %msg);
			MessageBoxOK("BLG-Ready", "You can now remotely control this server!");
	}
}

package BLG_GRSC {
	function GuiMouseEventCtrl::onMouseUp(%this, %mod, %pos, %click) {
		parent::onMouseUp(%this, %mod, %pos, %click);

		if(%this.BLGSH) {
			%obj = BLG_GRSC.getObject(%this.id);
			if(!isObject(%obj.gui)) {
				if(%obj.privateKey $= "") {
					MessageBoxOK("Error", "You have not yet acquired this server's private key. Joining the server will automatically download it.");
				} else {
					%gui = %obj.gui = BLG_GOO.newInstance(BLG_RemoteControl);
					%gui.part["servername"].setText(%obj.name @ " (CID: " @ %obj.cid @ ")");
					%gui.handle = %obj;
					BLG_GSC.relay(%obj.cid, "servercontrol\tlistening", %obj.gea);
				}
			}
			BLG_GOO.closeGui(BLG_SelectServer);
		}
	}

	function onExit() {
		BLG_GRSC.saveKeys();
		parent::onExit();
	}
};
activatePackage(BLG_GRSC);

BLG_GRSC.loadKeys();