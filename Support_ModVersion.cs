// ============================================================
// Project				:	Support_ModVersion
// Author				:	Iban
// Description			:	Harmonious Connection Arguments.
//								Document written in monospace font
//								with 3-space tabbing.
// Note: Edited by Jincux/Scout31 (9789) to work with RTB4. Ephialtes,
// 		why u try to limit to 6 args?
// ============================================================

// These global variables are used to register your mod's
// version information.

// $cArg[8, mFloor($cArgs[8])] = "MYMOD" TAB $MyInfo;
// $cArgs[8]++;

// Copy this part over exactly. Nothing needs to be changed,
// except what is after the equal sign.
// Note: Use TAB to seperate Mod Name from Version.
// CityRPG uses a password along with a version for various reasons.
// 
// You may use as many fields as you want, but
// all data (for ALL MODS) must be under 255 characters.
// So PLEASE PLEASE PLEASE be considerate in your data consumption.
// Any more than 25 characters is largely excessive.

// If this support package is already running, return.
if(isPackage(Support_ModVersion_Client))
	return;

package Support_ModVersion_Client
{
	// Function used to compile the connection string.
	// Every mod is put on a different line ("\n" or NL).
	// Therefore, all characters (except \n) are allowed.
	function getConnectArg(%arg)
	{
		// Declare variable.
		%connectString = "";
		
		// Cycle through all arguments.
		for(%i = 0; %i < $cArgs[%arg]; %i++)
			// If we're not the first argument, concat with a new line.
			if(%i)
				%connectString = %connectString NL $cArg[%arg, %i];
			// If we are, do not concat.
			else
				%connectString = $cArg[%arg, %i];
		
		// Return connection argument.
		return %connectString;
	}

	// According to the Appendix, 15 arguments is the limit.
	// If you try to do 16, the client outputs a console error
	// citing that you did not properly set connect args, and
	// the server will deny your connection.
	// 
	// If you are writing a mod and using this function,
	// PLEASE copy ALL the parameters so your package does not
	// break others.
	//
	// Argument #6 is used by RTB - that shit is holy, don't touch it.
	// Not sure what Argument #5 is used for.
	function GameConnection::setConnectArgs(%client, %lan, %net, %prefix, %suffix, %arg5, %rtb, %arg7, %arg8, %arg9, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15)
	{
		// The sweet spot here is Argument #8.
		// Set %arg8 equal to the connection string.
		%arg[8] = getConnectArg(8);
		parent::setConnectArgs(%client, %lan, %net, %prefix, %suffix, %arg5, %rtb, %arg7, %arg8, %arg9, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15);
	}
};

// RTB and DRPG do not package setConnectArgs ethically.
// Toss the package salad as to set this method at the bottom.
if(!$PackageToss)
{
	$PackageToss = true;
	
	// Deactivate
	if(isPackage(DRPG_Client))
		deactivatePackage(DRPG_Client);

	if(isPackage(RTB_Client))
		deactivatePackage(RTB_Client);

	activatePackage(Support_ModVersion_Client);

	// Re-Activate
	if(isPackage(DRPG_Client))
		activatePackage(DRPG_Client);

	if(isPackage(RTB_Client))
		activatePackage(RTB_Client);
	
	// Technical Explanation:
	// This package needs to be activated before the others,
	// because this mod passes 16 parameters to the engine,
	// instead of only 7 or 8, as DRPG and RTB do.
	// 
	// Consider it like this...
	// If this package is activated after the others,
	// we are essentially a large tube feeding into a
	// smaller one. Even if we can handle more water,
	// the other wont and the flow will be stopped.
	// Whereas smaller tubes can feed into a larger one,
	// and nothing bad happens and no info is lost.
}

// Returning here because the rest of the document is
// support on how to get Support_ModVersio running on a
// server.cs, not for a client.cs.
return;

// Put this function in your server package.
// Again, we're packaging all 15 parameters here so everything works.
function GameConnection::onConnectRequest(%client, %ip, %lan, %net, %prefix, %suffix, %arg5, %rtb, %arg7, %arg8, %arg9, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15)
{
	// Cycle through each line of %arg8.
	for(%a = 0; %a < getLineCount(%arg[8]); %a++)
	{
		// Get the %a'th light.
		%line = getLine(%arg[8], %a);
		
		// Test the first field of the line to your mod name.
		// Obviously, you'll want to replace MODDNAME with your identifier string.
		// Remember to keep it short.
		if(getField(%line, 0) $= "MODNAME")
		{
			// The version will be the second field.
			// Remember, getField's index is zero-based, sot he 2nd field == 1
			%version = getField(%line, 1);
			// mFloathLength adds and limits decimals to the specified number.
			// This would turn "3" to "3.0" and "3.01" to "3.0"
			// If you do not want this courtesy, remove or comment out
			// thsi lnie.
			%version = mFloathLength(%line, 1);
			
			// Set some client variables so we know that the client has our mod.
			// Again, obviously, you'll want to change the variable names.
			%client.hasMyMod = true;
			%client.myModVersion = %version;
			
			// Break the cycle to conserve CPU.
			// We already have what we want.
			break;
		}
	}
	
	// Properly package and return the result of the parent.
	return parent::onConnectRequest(%client, %ip, %lan, %net, %prefix, %suffix, %arg5, %rtb, %arg7, %arg8, %arg9, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15);
}