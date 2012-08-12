new ScriptObject(Bite) {
	class = Byte;
};
new ScriptObject(Ascii);

%fo = new FileObject();
%fo.openForRead("Add-Ons/System_BlocklandGlass/ascii.txt");
while(!%fo.isEOF()) {
	%line = %fo.readLine();
	%int = Bite.setHex(getWord(%line, 0)).getInteger();

	Ascii.hex[getWord(%line, 0)] = getWord(%line, 1);
	Ascii.int[%int] = getWord(%line, 1);
	Ascii.char[getWord(%line, 1)] = %int;
}
Ascii.hex["20"] = " ";
Ascii.int[32] = " ";
Ascii.char[" "] = 32;

Ascii.hex["09"] = "\t";
Ascii.int[9] = "\t";
Ascii.char["\t"] = 9;

Ascii.hex["0A"] = "\n";
Ascii.int[10] = "\n";
Ascii.char["\n"] = 10;

Ascii.hex["0D"] = "\r";
Ascii.int[13] = "\r";
Ascii.char["\r"] = 13;
%fo.close();
%fo.delete();