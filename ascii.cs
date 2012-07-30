new ScriptObject(Bite) {
	class = Byte;
};
new ScriptObject(Ascii);

%fo = new FileObject();
%fo.openForRead("Add-Ons/System_BlocklandGlass/ascii.txt");
while(!%fo.isEOF()) {
	%line = %fo.readLine();
	Bite.setHex(getWord(%line, 0));
	Ascii.hex[getWord(%line, 0)] = getWord(%line, 1);
	Ascii.int[Bite.getInteger()] = getWord(%line, 1);
	Ascii.char[getWord(%line, 1)] = Bite.getInteger();
}
Ascii.hex[20] = " ";
Ascii.int[32] = " ";
Ascii.char[" "] = 32;
%fo.close();
%fo.delete();