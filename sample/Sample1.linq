<Query Kind="Program" />

void Main()
{
	var lines = File.ReadAllLines(@"C:\data\favorites.txt");
	foreach (var line in lines)
	{
		// tab 쪼개기
		var name = line.Split('\t');
		// 3번 == 최재영
		if (name[2] == "최재영")
		{
			// 2번 출력
			name[1].Dump();
		}
	}
}
