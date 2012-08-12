//================================================
// Title: Glass Encryption Algorithm
//================================================

new ScriptObject(GEA);

function byteCheck() {
	%byte = new ScriptObject() { class = Byte; };
	for(%i = 0; %i < 256; %i++) {
		%byte.setInteger(%i);
	}
}

function isUpperCase(%char)
{
	%capitalized = StrUpr(%char); 			// Converts string to upper case.
	%result = StrCmp(%capitalized, %char);	// Compares two strings case sensitive.

	if (%result == 0)
		return true;
	
	return false;
}

function GEA::setKey(%this, %key) {
	if(strLen(%key) != 512) return 0;

	%byte = new ScriptObject() { class = Byte; };
	%byte2 = new ScriptObject() { class = Byte; };
	for(%i = 0; %i < 256; %i++) {
		%char = getSubStr(%key, %i*2, 2);
		%byte.setHex(%char);
		%byte2.setInteger(%i);
		if(%this.hex[%byte.getHex()] $= "") {
			%this.char[isUpperCase(%byte2.getChar()) ? "_" @ %byte2.getChar() : %byte2.getChar()] = %byte.getHex();	//Char, Hex
			%this.int[%i] = %byte.getHex();					//Int, Hex
			%this.hex[%byte.getHex()] = %byte2.getChar();	//Hex, Char
			%this.hexi[%byte.getHex()] = %i;				//Hex, Int
			%key = %key @ %byte.getHex();
		} else {
			return -1;
		}
	}
	%byte.delete();
	%byte2.delete();
	return 1;

}

function GEA::newKey(%this) {
	%byte = new ScriptObject() { class = Byte; };
	%byte2 = new ScriptObject() { class = Byte; };
	for(%i = 0; %i < 256; %i++) {
		%byte.setInteger(getRandom(0, 255));
		%byte2.setInteger(%i);
		if(%this.hex[%byte.getHex()] $= "") {
			%this.char[isUpperCase(%byte2.getChar()) ? "_" @ %byte2.getChar() : %byte2.getChar()] = %byte.getHex();	//Char, Hex
			%this.int[%i] = %byte.getHex();					//Int, Hex
			%this.hex[%byte.getHex()] = %byte2.getChar();	//Hex, Char
			%this.hexi[%byte.getHex()] = %i;				//Hex, Int
			%key = %key @ %byte.getHex();
		} else {
			%i--; //Loop again
		}
	}
	%byte.delete();
	%byte2.delete();
	return %this.key = trim(%key);
}

function GEA::encrypt(%this, %msg) {
	%inc = 0;
	for(%i = 0; %i < strLen(%msg); %i++) {
		%r = isUpperCase(getSubStr(%msg, %i, 1)) ? "_" @ getSubStr(%msg, %i, 1) : getSubStr(%msg, %i, 1);
		%enc = %enc @ %this.int[mMod(%this.hexi[%this.char[%r]] + %inc, 256)];
		%inc += %this.hexi[%this.char[%r]];
	}
	return %enc;
}

function GEA::decrypt(%this, %enc) {
	%inc = 0;
	for(%i = 0; %i < strLen(%enc); %i += 2) {
		%h = getSubStr(%enc, %i, 2);
		%msg = %msg @ %this.hex[%this.int[mMod(%this.hexi[%h] - %inc, 256)]];
		%inc += mMod(%this.hexi[%h] - %inc, 256);
	}
	return %msg;
}
