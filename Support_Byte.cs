//Multi-byte key example:
//0010 1110 - 0100 1110 = 11854
//46        - 78
//(46*256)+78 = 11854

//length = (keySize-1) / 8
//length = (16 - 1) / 8 = 2

function Byte::getInteger(%this) {
	if(%this.bit[8]) %i += 128;
	if(%this.bit[7]) %i += 64;
	if(%this.bit[6]) %i += 32;
	if(%this.bit[5]) %i += 16;
	if(%this.bit[4]) %i += 8;
	if(%this.bit[3]) %i += 4;
	if(%this.bit[2]) %i += 2;
	if(%this.bit[1]) %i += 1;
	return %i;
}

function Byte::getHex(%this) {
	%char[1] = 0;
	%char[2] = 0;
	%o = "0123456789ABCDEF";
	%int = %this.getInteger();
	%i = mfloor(%int/16);
	%char[1] = getSubStr(%o, %i, 1);
	%char[2] = getSubStr(%o, %int-(%i*16), 1);
	return %char[1] @ %char[2];
}

function Byte::getByte(%this) {
	return %this.byte;
}

function Byte::getChar(%this) {
	%int = %this.getInteger();
	if(%int > 127) {
		return -1;
	}

	return Ascii.int[%int] !$= "" ? Ascii.int[%int] : -1;
}

function Byte::setHex(%this, %hex) {
	%o = "0123456789ABCDEF";
	%char1 = getSubStr(%hex, 0, 1);
	%char2 = getSubStr(%hex, 1, 1);
	%int += strPos(%o, %char1)*16;
	%int += strPos(%o, %char2);
	%this.setInteger(%int);
}

function Byte::setInteger(%this, %i) {
	if(%i > 255) {
		return -1;
	}
	%this.bit[%j = 1] = "0";
	%this.bit[%j++] = "0";
	%this.bit[%j++] = "0";
	%this.bit[%j++] = "0";
	%this.bit[%j++] = "0";
	%this.bit[%j++] = "0";
	%this.bit[%j++] = "0";
	%this.bit[%j++] = "0";

	
	if(%i-128 >= 0 && !%this.bit[8]) {
		%this.bit[8] = 1;
		%i -= 128;
	}

	if(%i-64 >= 0 && !%this.bit[7]) {
		%this.bit[7] = 1;
		%i -= 64;
	}

	if(%i-32 >= 0 && !%this.bit[6]) {
		%this.bit[6] = 1;
		%i -= 32;
	}

	if(%i-16 >= 0 && !%this.bit[5]) {
		%this.bit[5] = 1;
		%i -= 16;
	}

	if(%i-8 >= 0 && !%this.bit[4]) {
		%this.bit[4] = 1;
		%i -= 8;
	}

	if(%i-4 >= 0 && !%this.bit[3]) {
		%this.bit[3] = 1;
		%i -= 4;
	}

	if(%i-2 >= 0 && !%this.bit[2]) {
		%this.bit[2] = 1;
		%i -= 2;
	}

	if(%i-1 >= 0 && !%this.bit[1]) {
		%this.bit[1] = 1;
		%i -= 1;
	}
	%this.byte = %this.bit[8] @ %this.bit[7] @ %this.bit[6] @ %this.bit[5] @ %this.bit[4] @ %this.bit[3] @ %this.bit[2] @ %this.bit[1];
	return %this;
}

function ByteGroup::fromInteger(%this, %int) {
	while(!%found) {
		%bytes++;
		if(%int < mPow(256, %bytes)) {
			%found = true;
		}
	}
	echo(%bytes SPC "bytes");

	for(%i = %bytes; %i > 0; %i--) {
		%b = new ScriptObject() {
			class = Byte;
		};
		%a = 0;
		%b.bit[%a++] = "0";
		%b.bit[%a++] = "0";
		%b.bit[%a++] = "0";
		%b.bit[%a++] = "0";
		%b.bit[%a++] = "0";
		%b.bit[%a++] = "0";
		%b.bit[%a++] = "0";
		%b.bit[%a++] = "0";

		if(%int-128*mPow(256, %i-1) >= 0 && !%b.bit[8]) {
			%b.bit[8] = 1;
			%int -= 128*mPow(256, %i-1);
		}

		if(%int-64*mPow(256, %i-1) >= 0 && !%b.bit[7]) {
			%b.bit[7] = 1;
			%int -= 64*mPow(256, %i-1);
		}

		if(%int-32*mPow(256, %i-1) >= 0 && !%b.bit[6]) {
			%b.bit[6] = 1;
			%int -= 32*mPow(256, %i-1);
		}

		if(%int-16*mPow(256, %i-1) >= 0 && !%b.bit[5]) {
			%b.bit[5] = 1;
			%int -= 16*mPow(256, %i-1);
		}

		if(%int-8*mPow(256, %i-1) >= 0 && !%b.bit[4]) {
			%b.bit[4] = 1;
			%int -= 8*mPow(256, %i-1);
		}

		if(%int-4*mPow(256, %i-1) >= 0 && !%b.bit[3]) {
			%b.bit[3] = 1;
			%int -= 4*mPow(256, %i-1);
		}

		if(%int-2*mPow(256, %i-1) >= 0 && !%b.bit[2]) {
			%b.bit[2] = 1;
			%int -= 2*mPow(256, %i-1);
		}

		if(%int-1*mPow(256, %i-1) >= 0 && !%b.bit[1]) {
			%b.bit[1] = 1;
			%int -= 1*mPow(256, %i-1);
		}
		%string = %string SPC %b.byte = %b.bit[8] @ %b.bit[7] @ %b.bit[6] @ %b.bit[5] @ %b.bit[4] @ %b.bit[3] @ %b.bit[2] @ %b.bit[1];
		%this.byte[%bytes-%i+1] = %b;
	}
	echo(trim(%string));
	%this.bytes = %bytes;
}

function ByteGroup::fromString(%this, %string) {
	for(%i = 0; %i < strLen(%string); %i++) {
		%b = new ScriptObject(){class=Byte;};
		%in = Ascii.char[getSubStr(%string, %i, 1)];
		%b.setInteger(%in !$= "" ? %in : 32);
		%this.byte[%i+1] = %b;
	}
	%this.bytes = strLen(%string);
}

function ByteGroup::getInteger(%this) {
	for(%i = %this.bytes; %i > 0; %i--) {
		%int += %this.byte[%i].getInteger() * mPow(256, %i-1);
	}
	return %int;
}

function ByteGroup::encrypt(%this, %e, %n) {
	%int = %this.getInteger();
	%enc = mMod(mPow(%int, %e), %n);
	return %enc;
}

function ByteGroup::fromDecrypt(%this, %enc, %d, %n) {
	%dec = mMod(mPow(%enc, %d), %n);
	%this.fromInteger(%dec);
}

function ByteGroup::print(%this) {
	for(%i = 0; %i < %this.bytes; %i++) {
		%str = %str @ %this.byte[%i+1].getChar();
	}
	echo(%str);
}

function mMod(%x, %y) {
	%i = mFloor(%x/%y);
	return %x-(%i*%y);
}

function mUnNotify(%int) {
	%n = getSubStr(%int, 0, strPos(%int, "e"));
	%places = getSubStr(%int, strPos(%int, "e")+2, 3);
	%places -= getSubStr(%int, 1, strPos(%int, "e")-1);
	%string = strReplace(%n, ".", "");
	for(%i = 0; %i < %places; %i++) {
		%string = %string @ "0";
	}
}

exec("./ascii.cs");

new ScriptObject(BiteGroup){class = ByteGroup;};
BiteGroup.byte[1] = new ScriptObject(){class = Byte;}.setInteger(119);
BiteGroup.byte[2] = new ScriptObject(){class = Byte;}.setInteger(120);
//BiteGroup.byte[3] = new ScriptObject(){class = Byte;}.setInteger(121);
//BiteGroup.byte[4] = new ScriptObject(){class = Byte;}.setInteger(122);
BiteGroup.bytes = 2;