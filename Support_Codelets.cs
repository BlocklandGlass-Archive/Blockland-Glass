//================================
// Codelets Framework
// Version 1.0
//================================
return;

$Codelets::Temp::Version = "1.0";
$Codelets::Temp::VersionId = 1;

if(isObject(Codelets)) {
	//if(Codelets.versionId < $Codelets::Temp::VersionId) {
	//	Codelets.setLegacyMode($Codelets::Temp::VersionId);
	//	Codelets.setName("Codelets_Old");
	//}

	new ScriptObject(Codelets) {
		version = $Codelets::Temp::Version;
		versionId = $Codelets::Temp::VersionId;
	};
}

function Codelets::newRepository(%this, %address, %port) {
	//Format: blockland.zivle.com/codelets/ 80
	if((%p = strPos(%address, "/")) != -1) {
		%host = getSubStr(%address, 0, %p);
		%path = getSubStr(%address, %p, strLen(%address));
	} else {
		%host = %address;
		%path = "/";
	}

	if(%port $= "" || %port != %port-0) {
		%port = 80;
	}

	%repo = new ScriptObject() {
		class = "Codelet_Repo";
		manager = %this;

		host = %host;
		path = %path;
		port = %port;

		checked = false;
		tcp = "";
	};

	%tcp = new TCPObject() {
		manager = %this;

		version = %this.versionId;
		repo = %repo;
		response = "";
	};

	%repo.tcp = %tcp;

	while(%tcp.response $= "") {
		//Waiting on verification. This should all be happening during boot, should not freeze game
	}

	if(%tcp.response !$= "CODE\t200") {
		return 0;
	}


	//Sample:
	//animationlib-1.0.1|notificationlib-2.5
	%buffer = getSubStr(%tcp.response, strPos(%tcp.response, "\n\n")+2, strLen(%tcp.response));
	%data = strReplace(%buffer, "|", "\t");
	for(%i = 0; %i < getFieldCount(%data); %i++) {
		%c = strReplace(getField(%data, %i), "-", " ");
		%repo.object[getWord(%c, 0)] = getWord(%c, 1);
	}
	return %repo;
}

function Codelet_Repo::downloadCodelet(%this, %codelet) {
	
}

package Codelets_1 {
	function TCPObject::onConnected(%this) {
		if(%this.version == 1) {
			%post = "GET " @ %this.repo.path @ " HTTP/1.1";
			//%post = "GET " @ %this.repo.path @ "/repo.dat HTTP/1.1";
			%post = %post @ "\nHost: " @ %this.repo.host;
			%post = %post @ "\nUser-Agent: Codelets/" @ %this.manager.versionId;
			%post = %post @ "\n\n";
			%this.send(%post);
		} else {
			parent::onConnected(%this);
		} 
	}

	function TCPObject::onLine(%this, %line) {
		if(%this.version == 1) {
			%this.buffer = %this.buffer @ %line @ "\n";
		} else {
			parent::onLine(%this, %line);
		} 
	}

	function TCPObject::onDisconnect(%this) {
		if(%this.version == 1) {
			%this.response = "CODE" TAB getWord(getLine(%this.buffer, 0), 1);
		} else {
			parent::onDisconnect(%this);
		} 
	}

	function TCPObject::onConnectFailed(%this) {
		if(%this.version == 1) {
			%this.response = "CONNECTFAIL";
		} else {
			parent::onDNSFailed(%this);
		}
	}

	function TCPObject::onDNSFailed(%this) {
		if(%this.version == 1) {
			%this.response = "DNSFAIL";
		} else {
			parent::onDNSFailed(%this);
		}
	}
};
activatePackage(Codelets_1);