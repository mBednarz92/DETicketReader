using DETicketReader;
using Spectre.Console;

AnsiConsole.Write(
    new FigletText("DETicket Reader")
        .LeftJustified()
        .Color(Color.Green));

TLVManager tlvM = new TLVManager();
RSADecryptor decryptor= new RSADecryptor();

string menuOption = ""; 

while(menuOption != "Exit")
{
    Console.WriteLine();
    menuOption = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Menu:")
        .PageSize(10)
        .AddChoices(new[] {
            "Decode", "Show Raw Data", "Show Organized Data(TLV)",  "Exit"
        }));
    switch (menuOption)
    {
        case "Decode":
            Console.WriteLine(tlvM.vdvSignedTicketsArray.Count());
            Console.WriteLine(decryptor.GetDecryptedData(tlvM.tlvData));
            break;
        case "Show Raw Data":
            tlvM.ShowRawData();
            break;
        case "Show Organized Data(TLV)":
            tlvM.ShowOrganizedData();
            break;
        case "Exit":
            Console.WriteLine("Bye");
            break;
    }
}



