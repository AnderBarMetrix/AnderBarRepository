<?php
	// Некоторые арифметические операции с целыми числами
#Made By Andrei Khotko
/*Код полностью компилируется, проверено на сайте:
ideone.com/*/
	
//--------------------------------------

function GetSum()		// 3 обращения к глобальным переменным
{
	global $Numb_1, $Numb_2, $Sum;
	$Sum = $Numb_1 + $Numb_2;
}

function GetDifference($Numb_1, $Numb_2)	// 0 обращений
{
	$Diff = $Numb_1 - $Numb_2;	
	return $Diff;
}

function GetMult_incorrect_return() // 0 обращений
{
	$Mult = $Numb_1 * $Numb_2;
}

function GetMult_global_return($Numb_1, $Numb_2) // 1 обращение
{
	$GLOBALS['Mult'] = $Numb_1 * $Numb_2;
}

function GetDivision($Numb_1, $Numb_2, &$Div)	// 0 обращений
{
	$Div = $Numb_1 / $Numb_2;
}
//---------------------------------------------
function GetInvolution_use_BuiltIn_function() // 3 обращения
// Возводит Numb_1 в степень Numb_2
{
	global $Involution;
	$base = $GLOBALS['Numb_1'];
	$exponent = $GLOBALS['Numb_2'];
	$Involution = pow($base, $exponent);
}

function My_GetInvolution($bas, $exponent) // 0 обращений
// Работает только для натуральной $exponent
{
	$Result = 1;
	for ($i = 1; $i <= $exponent; $i++)
		$Result = $Result * $bas;

	return $Res;
}

function Factorial()
{
	global $n;
	$Result = 1;

	for($i = 2; $i <= n; $i++)
		$Result = $Result * i;

	return $Result;
	
}

//---------------- Start --------------------

$Numb_1 = 10;
$Numb_2 = 36;
$Sum;

// Возможно 3 обращения к глобальным переменным
GetSum();
echo "$Numb_1 + $Numb_2 = $Sum\n";

$Numb_1 = 594;
$Numb_2 = 86;

// Возможно 4 обращения
$Diff = GetDifference($Numb_1, $Numb_2);
echo "$Numb_1 - $Numb_2 = $Diff\n";

$Numb_1 = 22;
$Numb_2 = 6;
$Mult;

// Возможно 5 обращений
GetMult_incorrect_return();
echo "incorrect return Mult result = $Mult\n";

// Все еще 5
GetMult_global_return($Numb_1, $Numb_2);
echo "$Numb_1 * $Numb_2 = $Mult\n";

$Numb_1 = 15;
$Numb_2 = 2;

// Возможно 6 обращений
GetDivision($Numb_1, $Numb_2, $Div);
echo "$Numb_1 / $Numb_2 = $Div\n";

$Numb_1 = 2;
$Numb_2 = 8;
$Involution;

// Возможно 7 обращений
GetInvolution_use_BuiltIn_function();
echo "Not my func: " . $Numb_1 . "^" . $Numb_2 . " = $Involution\n";

$Numb_1 = 5;
$Numb_2 = 4;
// Возможно 7 обращений
$Involution = function My_GetInvolution($Numb_1, $Numb_2);
echo "My func: " . $Numb_1 . "^" . $Numb_2 . " = $Involution\n";

$n = 3;
$Result = Factorial();
echo "Factorial = $Result"

$ssss = 'I m the LITERAL !!!!!!!
function GetSum_global_return_and_params()
{
	global $Numb_1, $Numb_2, $Sum;
	$Sum = $Numb_1 + $Numb_2;
}';

echo $ssss;

//-----------------------------------------
/*It was very easy program
	my friend*/

?>

