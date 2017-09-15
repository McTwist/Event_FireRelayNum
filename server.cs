// ================
// Name		::	FireRelayNum
// Version	::	6
// ================
// Made by	::	McTwist
// Date		::	17-09-15
// Info		::	Event that choose what onRelay to use
// License	::	Free to use
// ================

// Functions

// Magically convert string to readable numbers
function FireRelayNum::parseNumbers(%num)
{
	%pass = "";
	for (%i = 0; %i < getWordCount(%num); %i++)
	{
		%value = getWord(%num, %i);
		// Multiple
		if ((%pos = strpos(%value, "-")) >= 0)
		{
			// Get start to end, making it to numbers
			%start = getSubStr(%value, 0, %pos) << 0;
			%end = getSubStr(%value, %pos + 1, strlen(%value)) << 0;
			
			// Single
			if (%start >= %end)
				%pass = %pass SPC mFloor(%start);
			// Multiple
			else
				for (%n = %start; %n <= %end; %n++)
					%pass = %pass SPC mFloor(%n);
		}
		// Single
		else
		{
			// Make to number
			%value <<= 0;
			%pass = %pass SPC mFloor(%value);
		}
	}
	%pass = trim(%pass);
	return %pass;
}

// When processing an input event
function SimObject::ProcessFireRelay(%obj, %process, %client)
{
	// Onhly check for those events we are interested in
	%count = getWordCount(%process);
	for (%j = 0; %j < %count; %j++)
	{
		%i = getWord(%process, %j);
		
		// Already processed
		if (%tempEvent[%i])
			continue;

		// Enabled event
		if (!%obj.eventEnabled[%i])
			continue;
		
		// Not onRelay
		if (%obj.eventInput[%i] !$= "onRelay")
			continue;
		
		// Self
		if (%obj.eventTargetIdx[%i] == -1)
		{
			%type = "fxDTSBrick";
			%bricks = getBricksFromName(%obj.eventNT[%i]);
		}
		// Target another brick
		else
		{
			%type = inputEvent_GetTargetClass(%obj.getClassName(), %obj.eventInputIdx[%i], %obj.eventTargetIdx[%i]);
			%bricks = %obj;
		}

		// Parameters
		%numParams = outputEvent_GetNumParametersFromIdx(%type, %obj.eventOutputIdx[%i]);
		
		// Get parameters
		%param = "";
		for (%n = 1; %n <= %numParams; %n++)
			%param = %param @ ", \"" @ expandEscape(%obj.eventOutputParameter[%i, %n]) @ "\"";
		
		// Append client
		if (%obj.eventOutputAppendClient[%i] && isObject(%client))
			%param = %param @ ", " @ %client;
		
		// Go through list/brick
		%size = getWordCount(%bricks);
		for (%n = 0; %n < %size; %n++)
		{
			%next = getWord(%bricks, %n);
			
			// Call for event function
			// Note: There is no other feasible way to do this on
			eval("%event = %next.schedule(" @ %obj.eventDelay[%i] @ ", " @ %obj.eventOutput[%i] @ %param @ ");");
			
			// To be able to cancel an event
			%obj.addScheduledEvent(%event);
		}

		// Mark as processed
		%tempEvent[%i] = 1;
	}
}

// Events

registerOutputEvent(fxDTSBrick, "fireRelayNum", "string 100 100" TAB "list Brick 0 Up 1 Down 2 North 3 East 4 South 5 West 6", true);
registerOutputEvent(fxDTSBrick, "fireRelayRandomNum", "string 100 100" TAB "list Brick 0 Up 1 Down 2 North 3 East 4 South 5 West 6", true);

function fxDTSBrick::fireRelayNum(%brick, %num, %dir, %client)
{
	if (%num $= "")
		return %brick;
	
	// Get numbers
	%pass = FireRelayNum::parseNumbers(%num);
	
	if (%pass $= "")
		return %brick;
	
	// Brick
	if (%dir $= "0")
		%brick.ProcessFireRelay(%pass, %client);
	// Direction
	else
	{
		%bricks = %brick.getBricksDir(%dir);
		
		%size = getWordCount(%bricks);
		for (%i = 0; %i < %size; %i++)
		{
			%next = getWord(%bricks, %i);
			
			// No events, do nothing
			if (%next.numEvents == 0)
				continue;
			
			%next.ProcessFireRelay(%pass, %client);
		}
	}
	
	return %brick;
}

function fxDTSBrick::fireRelayRandomNum(%brick, %num, %dir, %client)
{
	if (%num $= "")
		return %brick;
	
	// Get numbers
	%pass = FireRelayNum::parseNumbers(%num);
	
	if (%pass $= "")
		return %brick;
	
	// Randomly pick one
	// Note: If you got the same number several times, then the chance is
	// higher for that number to be relayed
	%pass = getWord(%pass, getRandom(0, getWordCount(%pass) - 1));
	
	// Brick
	if (%dir $= "0")
		%brick.ProcessFireRelay(%pass, %client);
	// Direction
	else
	{
		%bricks = %brick.getBricksDir(%dir);
		
		%size = getWordCount(%bricks);
		for (%i = 0; %i < %size; %i++)
		{
			%next = getWord(%bricks, %i);
			
			// No events, do nothing
			if (%next.numEvents == 0)
				continue;
			
			%next.ProcessFireRelay(%pass, %client);
		}
	}
	
	return %brick;
}

// Package

if (isPackage(FireRelayNum))
	deactivatePackage(FireRelayNum);

package FireRelayNum
{
	// When adding an event
	function serverCmdAddEvent(%client, %enabled, %inputEvent, %delay, %target, %nameId, %outputEvent, %p0, %p1, %p2, %p3, %p4, %p5, %p6, %p7, %p8, %p9)
	{
		// Prevent schedule overflow
		if (%target <= 0 && $OutputEvent_NamefxDTSBrick_[%outputEvent] $= "fireRelayNum" && %delay < 33)
			%delay = 33;
		return Parent::serverCmdAddEvent(%client, %enabled, %inputEvent, %delay, %target, %nameId, %outputEvent, %p0, %p1, %p2, %p3, %p4, %p5, %p6, %p7, %p8, %p9);
	}
};
activatePackage(FireRelayNum);

// Support

// Get a list of bricks in a specific direction
function fxDTSBrick::getBricksDir(%brick, %dir)
{
	// Preparations
	%dir <<= 0;
	%bricks = "";
	
	// Get size
	%datablock = %brick.getDatablock();
	%rot = getWord(%obj.rotation, 3) % 180;
	if (%rot $= 90)
	{
		%sizex = %datablock.brickSizeY * 0.5;
		%sizey = %datablock.brickSizeX * 0.5;
	}
	else
	{
		%sizex = %datablock.brickSizeX * 0.5;
		%sizey = %datablock.brickSizeY * 0.5;
	}
	%sizez = %datablock.brickSizeZ * 0.2;
	%box = %sizex SPC %sizey SPC %sizez;
	
	// Get position
	%pos = %brick.getPosition();
	
	// Directions
	// North = y
	// South = -y
	// East = x
	// West = -x
	
	// Brick 0 Up 1 Down 2 North 3 East 4 South 5 West 6
	// Move box
	switch(%dir)
	{
		// Up
		case 1:
			%resize = -0.1 SPC -0.1 SPC 0;
			%move = 0 SPC 0 SPC 0.1;
		// Down
		case 2:
			%resize = -0.1 SPC -0.1 SPC 0;
			%move = 0 SPC 0 SPC -0.1;
		// North
		case 3:
			%resize = -0.1 SPC 0 SPC -0.1;
			%move = 0 SPC 0.1 SPC 0;
		// East
		case 4:
			%resize = 0 SPC -0.1 SPC -0.1;
			%move = 0.1 SPC 0 SPC 0;
		// South
		case 5:
			%resize = -0.1 SPC 0 SPC -0.1;
			%move = 0 SPC -0.1 SPC 0;
		// West
		case 6:
			%resize = 0 SPC -0.1 SPC -0.1;
			%move = -0.1 SPC 0 SPC 0;
		// Brick
		default:
			%bricks = %brick;
			return %bricks;
	}
	
	%pos = vectorAdd(%pos, %move);
	%box = vectorAdd(%box, %resize);
	
	// Get bricks
	InitContainerBoxSearch(%pos, %box, $Typemasks::FxBrickAlwaysObjectType);
	
	%evaluated[%brick] = 1;
	
	while (isObject(%next = containerSearchNext()))
	{
		// Avoid duplicates
		if (%evaluated[%next])
			continue;
		
		%evaluated[%next] = 1;
		
		%bricks = %bricks SPC %next;
	}
	
	return trim(%bricks);
}

// Get a list of bricks with the same name
function getBricksFromName(%name)
{
	%bricks = "";
	// Locate and rename
	while (isObject(%brick = %name))
	{
		%id = %brick.getID();
		%bricks = %bricks SPC %id;
		%id.setName(%name @ "_");
	}
	
	%bricks = trim(%bricks);
	// Reset names
	%size = getWordCount(%bricks);
	for (%i = 0; %i < %size; %i++)
	{
		getWord(%bricks, %i).setName(%name);
	}
	
	return %bricks;
}
