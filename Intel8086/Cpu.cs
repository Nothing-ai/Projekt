using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static Intel8086.RegisterType;
using static Intel8086.RegisterLetter;

namespace Intel8086
{
    public enum RegisterType { X, L, H }
    public enum RegisterLetter { A, B, C, D }

    public readonly struct Register
    {
        public RegisterType Type { get; init; }
        public RegisterLetter Letter { get; init; }

        public static Register FromName(string name) => new() {
            Letter = Enum.Parse<RegisterLetter>($"{name[0]}"),
            Type = Enum.Parse<RegisterType>($"{name[1]}")
        };

        public override string ToString() => $"{Letter}{Type}";

        public static Register AX = new() {Letter = A, Type = X};
        public static Register BX = new() {Letter = B, Type = X};
        public static Register CX = new() {Letter = C, Type = X};
        public static Register DX = new() {Letter = D, Type = X};

        public static Register AL = new() {Letter = A, Type = L};
        public static Register BL = new() {Letter = B, Type = L};
        public static Register CL = new() {Letter = C, Type = L};
        public static Register DL = new() {Letter = D, Type = L};

        public static Register AH = new() {Letter = A, Type = H};
        public static Register BH = new() {Letter = B, Type = H};
        public static Register CH = new() {Letter = C, Type = H};
        public static Register DH = new() {Letter = D, Type = H};
    }

    public enum Instruction
    {
        Mov, Push, Pop, Xchg
    };

    static class Cpu
    {
        private static Dictionary<RegisterLetter, int> Registers = new() { {A, 0}, {B, 0}, {C, 0}, {D, 0}};
        public static Stack<int> Stack = new();

        private const int HighMask = 0xff00, LowMask = 0x00ff, ReadMask = 0xffff;
        private const int Offset = 8;

        public static int Read(Register register) => register switch
        {
            { Type: X, Letter: var letter } => Registers[letter] & ReadMask,
            { Type: L, Letter: var letter } => Registers[letter] & LowMask,
            { Type: H, Letter: var letter } => (Registers[letter] >> Offset) & LowMask
        };

        public static void Store(Register register, int value)
        {
            switch (register.Type)
            {
                case X:
                    Registers[register.Letter] = value & ReadMask;
                    break;
                case L:
                    Registers[register.Letter] = (Registers[register.Letter] & HighMask) | (value & LowMask);
                    break;
                case H:
                    Registers[register.Letter] = ((value << Offset) & HighMask) | (Registers[register.Letter] & LowMask);
                    break;
            }
        }

        private static bool ValidateCompability(Register a, Register b) => (a.Type, b.Type) switch
        {
            (X, X)  => true,
            (H or L, H or L)  => true,
            _ => false
        };

        private static int Size(Register register) => register.Type switch { X => 16, _ => 8 };

        public static void Xchg(Register source, Register destination)
        {
            if (!ValidateCompability(source, destination))
            {
                MessageBox.Show($"Error: trying to swap value from {Size(source)}bit register {source} to {Size(destination)}bit register {destination}");
                return;
            }

            var s = Read(source);
            var d = Read(destination);
            Store(source, d);
            Store(destination, s);
        }
        public static void Mov(Register source, Register destination)
        {
            if (!ValidateCompability(source, destination))
            {
                MessageBox.Show($"Error: trying to move value from {Size(source)}bit register {source} to {Size(destination)}bit register {destination}");
                return;
            }
            Store(destination, Read(source));
        }

        public static void Push(Register source)
        {
            if (source.Type != X)
            {
                MessageBox.Show($"Error: trying to push 8bit value from register {source}");
                return;
            }
            Stack.Push(Read(source));
        } 

        public static void Pop(Register destination)
        {
            if (Stack is {Count: 0})
            {
                MessageBox.Show("Error: trying to pop from empty stack");
                return;
            }

            if (destination.Type != X)
            {
                MessageBox.Show($"Error: trying to pop 16bit value to 8bit register {destination}");
                return;
            }

            Store(destination, Stack.Pop());
        }
    }
}
