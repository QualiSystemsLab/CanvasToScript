/// <param name="commandOuput">
/// </param>
/// <param name="outValue">
/// </param>
PowerShell2Matrix(string commandOuput, out string [,] outValue)
#region /* User defined variables */
double n = 0
double m = 0
string[] tempVector = "[]"
string commandOuput = ""
string[,] outValue = "[;]"
double[] columnArray = [0]
#endregion

/* Main flow/s */
{
#region /* ------ Thread Start ------ */
    /* Start (Start) */
    ToVector(Source1: {commandOuput}, out Matrix /*[Create Variable]*/)
    /*Case: Case1*/
    if (length({ToVector.Matrix}) != 0)
    {
        MatShell1()
        ToMatrix(Source1: {ToVector.Matrix}, out Matrix: {outValue})
    }
    else
    {
        
    }
#endregion /* ------- Thread End ------- */
}

/* Functions */
ToVector()
{
/* ------ Transformation Flows ------ */
/* Flow 0 (Input: Source1, Output: Matrix) */
/* Step 0 */
String To Vector - By Delimiter(Delimiter: "
", Regular Expression: False, Start at index: 0, Treat consecutive delimiters as one: False, Text qualifier: None)

/*------ Transformation Flows End ------ */
}
MatShell1()
{
/* ------ Matshell code ------ */
/*noRecursion='----    ----    --'
columnArray=[]
command=substring(noRecursion,0,2)
command=''
for(n=1:(length(noRecursion)-1))
	command=command+'|' + substring(noRecursion,(n-1),2)
   if(strcmp(substring(noRecursion,(n-1),2),' -')==1)
     /*columnArray(length(columnArray))=n*//*
   end
end
*/

columnArray=[0]
for(n=1 : (length(ToVector.Matrix(1)) - 1))
if (strcmp(substring(ToVector.Matrix(1),(n - 1),2),' -') == 1)
columnArray(length(columnArray))=n
end
end
/* ---- Matshell code end ---- */
}
ToMatrix()
{
/* ------ Transformation Flows ------ */
/* Flow 0 (Input: Source1, Output: Matrix) */
/* Step 0 */
Pad with character(Padding directions: Right, Total length: length({ToVector.Matrix(0)}), Padding character: " ")
/* Step 1 */
Custom - Advanced(Expression: for ({n} = (0 : (length({input}) - 1)))
{tempVector} = [""]
for ({m} = (1 : (length({columnArray}) - 1)))
{tempVector(length({tempVector}))} = substring({input({n})})
end
{tempVector(length({tempVector}))} = substring({input({n})})
{output} = concatvertical({output})
end
, Output Type: String, Output Dimension: Matrix)
/* Step 2 */
Remove Row from Matrix By Index(Row Index: 1)
/* Step 3 */
Remove Empty Rows or Columns(Options: Columns)
/* Step 4 */
Trim Whitespace(Options: TrimBoth)

/*------ Transformation Flows End ------ */
}
