<?php
	$u_id = $_POST["Input_id"];
	$u_pw = $_POST["Input_pw"];
	$nick = $_POST["Input_nick"];

	if( empty($u_id) )
		die("u_id is empty.\n");

	if(empty($u_pw))
		die("u_pw is empty.\n");

	if(empty($nick))
		die("nick is empty.\n");

	//echo "$u_id<br/>";
	//echo "$u_pw<br/>";
	//echo "$nick<br/>";

	$con = mysqli_connect("localhost", "typhoon", "jun3824!", "typhoon");

	if( !$con)
		die( "Could not connect" . mysqli_connect_error() );

	$check = mysqli_query($con, "SELECT user FROM NumberGame WHERE user = '{$u_id}' ");
	$numrows = mysqli_num_rows($check);
	if($numrows != 0)
	{	
		die("ID does exist.");
	}

	$check = mysqli_query($con, "SELECT nick_name FROM NumberGame WHERE nick_name = '{$nick}' ");
	$numrows = mysqli_num_rows($check);
	if($numrows != 0)
	{	
		die("Nickname does exist.");
	}

	$Result = mysqli_query($con, "INSERT INTO NumberGame (user, pass, nick_name) VALUES 
				( '{$u_id}', '{$u_pw}', '{$nick}' );" );

	if($Result)
		echo "Create Success.";
	else 
		echo "Create error.";

	mysqli_close($con);

?>  