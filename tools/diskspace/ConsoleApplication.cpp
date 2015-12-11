// ConsoleApplication1.cpp : Defines the entry point for the console application.
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

int main(int /*argc*/, char ** /*argv*/)
{
	ULARGE_INTEGER free_bytes_available;
	ULARGE_INTEGER total_number_of_bytes;
	ULARGE_INTEGER total_number_of_free_bytes;

	const BOOL success = GetDiskFreeSpaceEx(NULL, &free_bytes_available, &total_number_of_bytes, &total_number_of_free_bytes);
	if (!success)
	{
		const DWORD error = GetLastError();
		print_message_line("******************************************");
		print_message_line("Error getting disk space information", error);
		print_message_line("******************************************");
		return EXIT_FAILURE;
	}

	const DWORDLONG available = free_bytes_available.QuadPart / KB_DIV;
	const DWORDLONG total = total_number_of_bytes.QuadPart / KB_DIV;
	const DWORDLONG total_free = total_number_of_free_bytes.QuadPart / KB_DIV;

	print_message_line("******************************************");
	print_message_line("Disk space available", available);
	print_message_line("Total disk space", total);
	print_message_line("Total disk space available", total_free);
	print_message_line("******************************************");

	return EXIT_SUCCESS;
}
