// ConsoleApplication.cpp : Defines the entry point for the console application.
//

#include <iostream>
#include <string>
#include <Windows.h>

static const int KB_DIV = 1024;
static const int MESSAGE_WIDTH = 30;
static const int HELP_WIDTH = 20;
static const int NUMERIC_WIDTH = 10;

void print_message(std::string const & msg, bool addColon = false)
{
	std::cout.width(MESSAGE_WIDTH);
	std::cout << msg;
	if (addColon)
	{
		std::cout << ":";
	}
}

void print_message_line(std::string const & msg)
{
	print_message(msg);
	std::cout << std::endl;
}

void print_message_line(std::string const & msg, DWORD value)
{
	print_message(msg, true);
	std::cout.width(NUMERIC_WIDTH);
	std::cout << std::right << value << std::endl;
}

void print_message_line(std::string const & msg, DWORDLONG value)
{
	print_message(msg, true);
	std::cout.width(NUMERIC_WIDTH);
	std::cout << std::right << value << std::endl;
}

bool get_option(int argc, char **argv, std::string const & option)
{
	for (int i = 0; i < argc; ++i)
	{
		if (option.compare(argv[i]) == 0)
		{
			return true;
		}
	}
	return false;
}

void show_help()
{
	std::cout << "Usage:" << std::endl;
	std::cout.width(HELP_WIDTH);
	std::cout << "--help : ";
	std::cout.width(HELP_WIDTH);
	std::cout << std::right << "display this message" << std::endl;
	std::cout.width(HELP_WIDTH);
	std::cout << std::right << "-v (--verbose) : ";
	std::cout.width(HELP_WIDTH);
	std::cout << std::right << "display complete memory status information" << std::endl;
}

int main(int argc, char **argv)
{
	const bool display_help = get_option(argc, argv, "--help");
	if (display_help)
	{
		show_help();
		return EXIT_SUCCESS;
	}

	const bool display_all = get_option(argc, argv, "-v") || get_option(argc, argv, "--verbose");

	MEMORYSTATUSEX status;
	status.dwLength = sizeof(status);
	const BOOL success = GlobalMemoryStatusEx(&status);
	if (!success)
	{
		const DWORD error = GetLastError();
		print_message_line("******************************************");
		print_message_line("Error getting memory information", error);
		print_message_line("******************************************");
		return EXIT_FAILURE;
	}

	const DWORD load = status.dwMemoryLoad;
	const DWORDLONG physical = status.ullTotalPhys / KB_DIV;
	const DWORDLONG free_physical = status.ullAvailPhys / KB_DIV;
	const DWORDLONG page = status.ullTotalPageFile / KB_DIV;
	const DWORDLONG free_page = status.ullAvailPageFile / KB_DIV;
	const DWORDLONG virtual_memory = status.ullTotalVirtual / KB_DIV;
	const DWORDLONG free_virtual = status.ullAvailVirtual / KB_DIV;
	const DWORDLONG free_extended = status.ullAvailExtendedVirtual / KB_DIV;

	print_message_line("******************************************");
	print_message_line("Percent of memory in use", load);
	print_message_line("KB of physical memory", physical);
	print_message_line("KB of free physical memory", free_physical);
	if (display_all)
	{
		print_message_line("KB of paging file", page);
		print_message_line("KB of free paging file", free_page);
		print_message_line("KB of virtual memory", virtual_memory);
		print_message_line("KB of free virtual memory", free_virtual);
		print_message_line("KB of free extended memory", free_extended);
	}
	print_message_line("******************************************");

	return EXIT_SUCCESS;
}
