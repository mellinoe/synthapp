using System;
using System.Collections.Generic;

namespace SynthApp
{
    public class Project
    {
        public List<Channel> Channels { get; set; } = new List<Channel>();
        public List<Pattern> Patterns { get; set; } = new List<Pattern>();

        public static Project CreateDefault()
        {
            var channels = new List<Channel>();
            var tri = new SimpleOscillatorSynth();
            tri.Generator.Type = SimpleWaveformGenerator.WaveformType.Square;
            channels.Add(tri);

            var saw = new SimpleOscillatorSynth();
            saw.Generator.Type = SimpleWaveformGenerator.WaveformType.Sawtooth;
            channels.Add(saw);

            var sine = new SimpleOscillatorSynth();
            sine.Generator.Type = SimpleWaveformGenerator.WaveformType.Sine;
            channels.Add(sine);

            var kick = new WaveSampler(@"E:\Audio\vengeance essential club sounds-3\vengeance essential club sounds-3\VEC3 Bassdrums\VEC3 Clubby Kicks\VEC3 Bassdrums Clubby 001.wav");
            channels.Add(kick);
            kick.Gain = 0.85f;

            var pattern = new Pattern(channels);

            NoteSequence triPattern = new NoteSequence();
            triPattern.Notes.Add(new Note(PatternTime.Steps(2), PatternTime.Steps(2), new Pitch(PitchClass.A, 2)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(6), PatternTime.Steps(2), new Pitch(PitchClass.A, 2)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(10), PatternTime.Steps(2), new Pitch(PitchClass.A, 2)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(14), PatternTime.Steps(1), new Pitch(PitchClass.A, 2)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(15), PatternTime.Steps(1), new Pitch(PitchClass.A, 3)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(18), PatternTime.Steps(2), new Pitch(PitchClass.A, 2)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(22), PatternTime.Steps(2), new Pitch(PitchClass.A, 2)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(26), PatternTime.Steps(2), new Pitch(PitchClass.A, 2)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(30), PatternTime.Steps(1), new Pitch(PitchClass.A, 2)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(31), PatternTime.Steps(1), new Pitch(PitchClass.A, 3)));
            triPattern.UsesPianoRoll = true;
            pattern.NoteSequences[0] = triPattern;

            NoteSequence sawPattern = new NoteSequence();
            sawPattern.Notes.Add(new Note(PatternTime.Steps(0), PatternTime.Steps(2), new Pitch(PitchClass.CSharp, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(4), PatternTime.Steps(2), new Pitch(PitchClass.E, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(8), PatternTime.Steps(2), new Pitch(PitchClass.CSharp, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(12), PatternTime.Steps(2), new Pitch(PitchClass.E, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(14), PatternTime.Steps(1), new Pitch(PitchClass.FSharp, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(15), PatternTime.Steps(1), new Pitch(PitchClass.G, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(18), PatternTime.Steps(1), new Pitch(PitchClass.G, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(20), PatternTime.Steps(1), new Pitch(PitchClass.G, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(22), PatternTime.Steps(1), new Pitch(PitchClass.G, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(24), PatternTime.Steps(3), new Pitch(PitchClass.FSharp, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(28), PatternTime.Steps(3), new Pitch(PitchClass.E, 4)));
            sawPattern.UsesPianoRoll = true;
            pattern.NoteSequences[1] = sawPattern;

            NoteSequence sinePattern = new NoteSequence();
            sinePattern.Notes.Add(new Note(PatternTime.Steps(0), PatternTime.Steps(2), new Pitch(PitchClass.CSharp, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(4), PatternTime.Steps(2), new Pitch(PitchClass.E, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(8), PatternTime.Steps(2), new Pitch(PitchClass.CSharp, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(12), PatternTime.Steps(2), new Pitch(PitchClass.E, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(14), PatternTime.Steps(1), new Pitch(PitchClass.FSharp, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(15), PatternTime.Steps(1), new Pitch(PitchClass.G, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(18), PatternTime.Steps(1), new Pitch(PitchClass.G, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(20), PatternTime.Steps(1), new Pitch(PitchClass.G, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(22), PatternTime.Steps(1), new Pitch(PitchClass.G, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(24), PatternTime.Steps(3), new Pitch(PitchClass.A, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(28), PatternTime.Steps(1), new Pitch(PitchClass.E, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(29), PatternTime.Steps(1), new Pitch(PitchClass.D, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(30), PatternTime.Steps(1), new Pitch(PitchClass.CSharp, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(31), PatternTime.Steps(1), new Pitch(PitchClass.B, 5)));
            sinePattern.UsesPianoRoll = true;
            pattern.NoteSequences[2] = sinePattern;

            NoteSequence kicks = new NoteSequence();
            kicks.Notes.Add(new Note(new PatternTime(0, 0), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(new PatternTime(4, 0), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(new PatternTime(8, 0), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(new PatternTime(12, 0), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(new PatternTime(16, 0), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(new PatternTime(20, 0), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(new PatternTime(24, 0), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(new PatternTime(28, 0), PatternTime.Steps(2), Pitch.MiddleC));
            pattern.NoteSequences[3] = kicks;


            Project project = new Project();
            project.Channels = channels;
            project.Patterns = new List<Pattern>() { pattern };
            return project;
        }

        public int GetChannelIndex(Channel channel)
        {
            for (int i = 0; i < Channels.Count; i++)
            {
                if (Channels[i] == channel)
                {
                    return i;
                }
            }

            throw new InvalidOperationException("Invalid channel was not contained in the Project's array of Channels: " + channel);
        }
    }
}
