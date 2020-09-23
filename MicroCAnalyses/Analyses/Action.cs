namespace Analyses
{
    public enum Action
    {
        // Declarations
        IntDeclaration, 
        ArrayDeclaration, 
        RecordDeclaration, 
        
        // Assignments
        IntAssignment, 
        ArrayAssignment, 
        RecordMemberAssignment,
        
        // Input/Output
        Read, 
        Write,
        
        // Evaluating expressions
        ArithmeticExpression, 
        BooleanExpression
    }
}