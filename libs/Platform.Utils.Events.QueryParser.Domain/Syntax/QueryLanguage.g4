grammar QueryLanguage;

/*
 * Parser Rules
 */

//prog: expr+ ;

/*
expr1
	:	left = expr Dot right = expr			# Chain
	|	memberName methodInvocation			# Method
	|	memberName							# Token
	;
*/

queryChain
	:   queryChainItem (queryChainItem)*
	;

queryChainItem
	:   query (SemiColon | Comma)?
	;

query
	:   queryNode projection? (Dot queryNode projection?)*
//	|   rootNode (Dot queryExpression)+
//	|   rootNode queryNode
//	|   rootNode Dot method
	;


queryNode
	:   pivotExpression collection?
//	|   pivotExpression
//	|   rootNode
	|   rootNode collection?
	|   method
	|   property collection?
//	|   property
	;

rootNode
	:   (EData | variable | model)
//	:   (EData | variable | model) (Dot pivotValues filterExpresion)?
//	|   (EData | variable | model) filterExpresion?
	;

variable
	:   DollarSign variableName = Label
	;

model
	:   stringType
	;

property
	: Label
//	| keywords
	;

labelChain
	:   Label (Dot Label)*
	;


labelChainCollection
	:   labelChain (Comma labelChain)*
	;


labelPivot
	:	Label
	|   pivotExpression
//	|	pivotWords
	;


labelPivotChain
	:   labelPivot (Dot labelPivot)*
	;


pivotExpression
	:   pivotType = (Ae | Ts | Md | Hd | Vx | Bp | Ac | Vae | Vhd | Vmd | Vvx) (Dot pivotValues)?
//	|   pivotType = Vx (Dot pivotValues)? projection?
	|   rootNode (Dot pivotValues)
	;

pivotValues
	:	pivotValue1 = Label (Lt labelChainCollection? Gt)?
	;


projection
    :   OpenSquareBracket queryChain CloseSquareBracket
    ;

collection
    :   (OpenRoundBracket filterExpressions? CloseRoundBracket)
    ;

filterConditionSubject
//	:   labelPivotChain
	:   query
	;

filterConditionValue
    :   typeValue
    |   query
    ;

filterConditionGroupBegin
	:   OpenRoundBracket
	;

filterConditionGroupEnd
	:   CloseRoundBracket
	;

filterCondition
	:   subject = filterConditionSubject notModifier = Not? comparator = (Eq|NotEq|Gt|Ge|Lt|Le|In|Between|Like|BeginsWith|EndsWith) filterConditionValue
    |   notModifier = Not? subject = filterConditionSubject
//	:   appenderType = (And|Or)? subject = filterConditionSubject notModifier = Not? comparator = (Eq|NotEq|Gt|Ge|Lt|Le|In|Between|Like) filterConditionValue
//	|   appenderType = (And|Or)? notModifier = Not? subject = filterConditionSubject
//	|   appenderType = (And|Or)? filterConditionGroupBegin subject = filterConditionSubject notModifier = Not? comparator = (Eq|NotEq|Gt|Ge|Lt|Le|In|Between|Like) filterConditionValue filterConditionGroupEnd
//	|   appenderType = (And|Or)? filterConditionGroupBegin notModifier = Not? subject = filterConditionSubject filterConditionGroupEnd
	;

/*
filterConditionGroup
    :
    |
    ;
*/


filterExpressions
    :   filterConditionGroupBegin filterExpressions filterConditionGroupEnd   #filterExpressionGroup1
    |   filterExpressions appenderType = (And|Or) filterExpressions           #filterExpressionGroup
    |   filterCondition                                                       #filterExpressionGroupElement
    ;

filterExpresion1
	:   filterCondition
//	|   filterConditionGroupBegin filterExpresion* filterConditionGroupEnd
//	|   filterConditionGroupBegin? filterExpresion (appenderType = (And|Or) filterExpresion)* filterConditionGroupEnd?
	;

setArgumentSubject
	:   query
	;

setArgumentValue
	:   typeValue   // ._Set(a = 1)
	|   query       // ._Set(a = $x.vx.name)
	;

setArgument
	:   subject = setArgumentSubject Eq value = setArgumentValue
	;


addArgumentSubject
	:   query
	;

addArgumentValue
	:   typeValue   // ._Add(a = 1)
	|   query       // ._Add(a = $x.vx.name)
	;

addArgument
	:   subject = addArgumentSubject Eq value = addArgumentValue
	|   value = addArgumentValue
//	|   queryValue = query                //  .Add(md()._Set(x = 1))
	;

tomdArgument
	:   genericStringValueArgument
	;

toLocalArgument
	:   genericStringValueArgument
	;

takeArgument
	:   genericIntValueArgument
	;

skipArgument
	:   genericIntValueArgument
	;

orderByArgument
	:   genericQueryValueArgument
	;

genericIntValueArgument
    :   value = intType
    ;

genericStringValueArgument
    :   value = stringType
    ;

genericQueryValueArgument
    :   value = query
    ;

method
	:   methodType = Set OpenRoundBracket setArgument (Comma setArgument)* CloseRoundBracket
	|   methodType = Get OpenRoundBracket CloseRoundBracket
	|   methodType = GetValue OpenRoundBracket CloseRoundBracket
	|   methodType = Add OpenRoundBracket (pivotValues Comma)? (addArgument (Comma addArgument)*)? CloseRoundBracket
	|   methodType = ToMD OpenRoundBracket tomdArgument CloseRoundBracket
	|   methodType = ToT OpenRoundBracket CloseRoundBracket
	|   methodType = ToLocal OpenRoundBracket toLocalArgument CloseRoundBracket
	|   methodType = Delete OpenRoundBracket CloseRoundBracket
	|   methodType = Take OpenRoundBracket takeArgument CloseRoundBracket
	|   methodType = Skip OpenRoundBracket skipArgument CloseRoundBracket
	|   methodType = OrderBy OpenRoundBracket (orderByArgument (Comma orderByArgument)*) CloseRoundBracket
	;

array
	:	OpenSquareBracket (typeValue (Comma typeValue)*) CloseSquareBracket
	;

typeValue
	:	guidType
	|	intType
	|	hexType
	|	floatType
	|	booleanType
	|	stringType
	|	dateTimeType
	|	array
	;

booleanType
	:	TRUE
	|	FALSE
	;

stringType
	:	DoubleQuoteString
	|	SingleQuoteString
//	|   TriangleQuotedString
	;

guidType
	:	Guid
//	|	SingleQuoteGuid
//	|	DoubleQuoteGuid
	;

intType
	:	Minus? Integer
	;

hexType
	:	Hex
	;

floatType
	:	Minus? Float
	;

dateTimeType
	:	DateTime
	;

/*
 * Lexer Rules
 */


Colon:	':';
SemiColon:	';';
Dot:	'.';
Comma: 	',';
Minus: 	'-';
Plus: 	'+';
Underscore: 	'_';
DollarSign: 	'$';

Or:		'||';
And:	'&&';


Eq: 	'=';
NotEq: 	'!=';
Gt: 	'>';
Ge: 	'>=';
Lt: 	'<';
Le: 	'=<';
In: 	I N;
Like: 	L I K E;
BeginsWith: 	B E G I N S W I T H;
EndsWith: 	E N D S W I T H;
Between: 	B E T W E E N;
Not:    N O T;

//Label:              ('a'..'z' | 'A'..'Z' | '_')  ('a'..'z' | 'A'..'Z' | '0'..'9' | '_' )*;

DoubleQuote:    '"';
Quote:  '\'';


// Pivot words
Md:	'md';
Ae:	'ae';
Ts:	'ts';
Vx:	'vx';
Hd:	'hd';
Bp:	'bp';
Ac:	'ac';

Vae:	'vae';
Vvx:	'vvx';
Vmd:	'vmd';
Vhd:	'vhd';

// Keywords
//InputData:	I N P U T D A T A;
EData:	'eData';

// Variables
//x: 'x';
//y: 'y';
//z: 'z';


// Methods
Set:	Underscore S E T;
Add:	Underscore A D D;
Get:	Underscore G E T;
GetValue:	Underscore G E T V A L U E;
Delete:	Underscore D E L E T E;
ToMD: Underscore T O M D;
ToT: Underscore T O T;
ToLocal: Underscore T O L O C A L;
Any: Underscore A N Y;
Take: Underscore T A K E;
Skip: Underscore S K I P;
OrderBy: Underscore O R D E R B Y;
InitFrom: Underscore I N I T F R O M;

TRUE:	T R U E;
FALSE:	F A L S E;

OpenRoundBracket:   '(';
CloseRoundBracket:  ')';
OpenSquareBracket:  '[';
CloseSquareBracket: ']';

Label
	:   (LCaseLetter | UCaseLetter | Underscore) (LCaseLetter | UCaseLetter | Digit | Underscore)*
	;


fragment Digit:                [0-9];
fragment LCaseLetter:          [a-z];
fragment UCaseLetter:          [A-Z];
fragment HexDigit:             [a-fA-F0-9];

fragment SingleQuoteStringFragment: (~('\'' | '\\') | '\\' . )* ;
fragment DoubleQuoteStringFragment: (~('\\' | '"') | '\\' . )* ;
//fragment TriangleQuoteStringFragment: (~(',') . )* ;
fragment A: [aA];
fragment B: [bB];
fragment C: [cC];
fragment D: [dD];
fragment E: [eE];
fragment F: [fF];
fragment G: [gG];
fragment H: [hH];
fragment I: [iI];
fragment J: [jJ];
fragment K: [kK];
fragment L: [lL];
fragment M: [mM];
fragment N: [nN];
fragment O: [oO];
fragment P: [pP];
fragment Q: [qQ];
fragment R: [rR];
fragment S: [sS];
fragment T: [tT];
fragment U: [uU];
fragment V: [vV];
fragment W: [wW];
fragment X: [xX];
fragment Y: [yY];
fragment Z: [zZ];

//SingleQuoteGuid: Quote Guid Quote;
//DoubleQuoteGuid: DoubleQuote Guid DoubleQuote;

//TriangleQuotedString: Lt TriangleQuoteStringFragment Gt;
SingleQuoteString: Quote SingleQuoteStringFragment Quote;
DoubleQuoteString: DoubleQuote DoubleQuoteStringFragment DoubleQuote;


Hex:	'0' ('x'|'X') HexDigit+;

Integer:	Digit+;

Float:	Digit+ Dot Digit+;

Guid:	HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit Minus
		HexDigit HexDigit HexDigit HexDigit Minus
		HexDigit HexDigit HexDigit HexDigit Minus
		HexDigit HexDigit HexDigit HexDigit Minus
		HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit;

DateTime:	Digit Digit Digit Digit Minus
			Digit Digit Minus
			Digit Digit T
			Digit Digit Colon
			Digit Digit Colon
			Digit Digit;


WS
	:	[ \t\r\n]+ -> skip 
	;
