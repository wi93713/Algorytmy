   M S B A M L   `   `   `  ���� d  `Microsoft.Maps.MapControl.WPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35tRclr-namespace:Microsoft.Maps.MapControl.WPF;assembly=Microsoft.Maps.MapControl.WPFMicrosoft.Maps.MapControl.WPF  E AAlgorytmy2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:Algorytmy2
Algorytmy2    Algorytmy2.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     `mRclr-namespace:Microsoft.Maps.MapControl.WPF;assembly=Microsoft.Maps.MapControl.WPF     *localclr-namespace:Algorytmy2       � Title$  
MainWindow��$	��800��$
��1300��.��>� .���� .+��  � ColumnDefinitions �� $
��115* ��� $
��166* ��� $	��98* ��� $
��178* ��� $
��128* ��� $
��608* � � RowDefinitions �� $��20 ��� $	��26* ��� $	��29* ��� $
��695* �.+�]� $��1��$��6��.+�o� $	��Top=�.@�m� $
��Plik��.@�m� -   $��Otwórz��m� -   $��Zapisz���� m� $��_Exit��m� -   $��RouteABMenu��$��
Trasa A->B��.@�m� -   $%��Znajdź najkrótsza trasę A->B���� $��Wybierz Punkt A��$	��Top=�$��1��$��1��$��26���� -    ��Name$ cboxPunktStartowy��$��1�� �� 
IsEditable . $��1��$��0,0,0,0q����� *
m_Startowy+�� $��Max dystans:��$	��Top=�$��1��$��2��$��26���� $��km��$	��Top=�$��1��$��3��$��48,0,0,0q�$��26���� -    ��Name$ txboxMaxDistance��$��3��$
��Left=�$��26��$��0,0,0,0q�$��1�� �� TextWrapping$
 Wrap=�$��0��$	��Top=�$��48�� �� TextAlignment$ Center=�$��14(��� $��Wybierz Heurystyke��$
��Left=�$��0,0,0,0q�$��2��$	��Top=�$��26��$	��111���� -   $ cboxHeurystyka��$��2��$��1��$��0,0,0,0q����� *m_Heurystyka+�� -    ��Name$ 	btnOblicz��$��2��$��4��$��Oblicz��$��
0,0,11.5,0q�$��Right=�$	��106���� -	   	 ��Name$	 canvas��$
��Left=�$	��684��$��3��$	��Top=�$
��1282��$��6��$��1��.+�'   !Microsoft.Maps.MapControl.WPF.Map  -
   $��bingMap��$��1��$��3��+   %Microsoft.Maps.MapControl.WPF.MapCore
   CredentialsProviderT   NMicrosoft.Maps.MapControl.WPF.Design.ApplicationIdCredentialsProviderConverter$r
 lBe3OHm6pgS2WA1QDi0Rr~8JYfIRS-fNWCMmFolCa9Xg~AiCjkWu0uS4fFTwlxDiEEGMqND-gF6wi255eGYTc5AiD3GtHanZv0YOs6bV5MeE0 $y�Hidden=�$	��571��$	��986��
 �� Top$ 10�� �� Left$ 10���� -    ��Name$ labHeurystykaAddon��$��Liczba scieżek��$��2��$
��Left=�$��2��$	��Top=�$��26��$��98��$y�Hidden=��� -   $ txboxHeurystykaAddon��$��3��$
��Left=�$��26��$��2��$
 Wrap=�$
��1000��$	��Top=�$ Center=�$��14(�$��73��$y�Hidden=��� -   $ btnReset��$��5��$
��Left=�$��
21.5,1,0,0q�$��2��$��Reset��$	��Top=�$��86��$��25���� -   $ displayedOther��$��5��$
��Left=�$	��126��$��	20,28,0,0q�$��3��$	��Top=�$	��560��.���� -   $ displayedOtherText��$	��103��$
 Wrap=�$�� ��$	��517��$y�Hidden=��� -   $ displayedCities��$��5��$
��Left=�$	��190��$��
20,172,0,0q�$��3��$	��Top=�$	��560��.���� -   $ displayedCitiesText��$	��179��$
 Wrap=�$�� ��$	��557��$y�Hidden=�