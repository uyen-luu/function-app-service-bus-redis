﻿namespace IPS.Grow.Func.Models;

public class ProductMessage
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public int[] Categories { get; set; } = [];
}

public class ProductState : ProductMessage
{
    public DateTime? Created { get; set; }
    public DateTime? LastModified { get; set; }
}


public class ProductCategoryMessage
{
    public required string Name { get; set; }
}

public class ProductCategoryState : ProductCategoryMessage
{
    public DateTime? Created { get; set; }
    public DateTime? LastModified { get; set; }
}

