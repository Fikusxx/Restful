﻿namespace Library.Services;

public interface IPropertyCheckerService
{
	bool TypeHasProperties<T>(string fields);
}