package it.greenthumbz.gemma;

import android.os.Environment;
import android.speech.tts.TextToSpeech;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import java.io.File;
import java.io.IOException;
import java.util.Locale;
import java.util.UUID;


public class MainActivity extends AppCompatActivity {

    private static final String EXTERNAL_STORAGE = "Gemma";

    private TextToSpeech ttsService;
    private Button buttonPlay;
    private Button buttonSave;
    private EditText inputText;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        buttonPlay = findViewById(R.id.button_play);
        buttonSave = findViewById(R.id.button_speak);

        ttsService = new TextToSpeech(this, new TextToSpeech.OnInitListener() {
            @Override
            public void onInit(int status) {
                if (status == TextToSpeech.SUCCESS) {
                    int result = ttsService.setLanguage(Locale.ITALIAN);
                    if (result == TextToSpeech.LANG_MISSING_DATA || result == TextToSpeech.LANG_NOT_SUPPORTED) {
                        Toast.makeText(getApplicationContext(), "Italian language not supported.", Toast.LENGTH_SHORT).show();
                    }
                }
            }
        });

        inputText = findViewById(R.id.input_text);

        buttonPlay.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                String capturedText = inputText.getText().toString();
                if (! capturedText.isEmpty()) {
                    String utteranceId = UUID.randomUUID().toString();
                    int speechStatus = ttsService.speak(capturedText, TextToSpeech.QUEUE_FLUSH, null, utteranceId);
                    if (speechStatus == TextToSpeech.ERROR) {
                        Toast.makeText(getApplicationContext(), "TTS speak: Error during the conversion.", Toast.LENGTH_SHORT).show();
                    }
                } else {
                    Toast.makeText(getApplicationContext(), "Please, enter a message", Toast.LENGTH_SHORT).show();
                }
            }
        });

        buttonSave.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                String capturedText = inputText.getText().toString();
                if (! capturedText.isEmpty()) {
                    File rootDir = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOWNLOADS), EXTERNAL_STORAGE);
                    if (! rootDir.exists()) {
                        rootDir.mkdirs();
                    }
                    File audiofile = new File(rootDir, "speech.mp3");
                    try {
                        audiofile.createNewFile();
                        audiofile.setWritable(true);
                        int synthStatus = ttsService.synthesizeToFile((CharSequence) capturedText, null, audiofile, null);
                        if (synthStatus == TextToSpeech.ERROR_SYNTHESIS) {
                            Toast.makeText(getApplicationContext(), "TTS synth: Something went wrong", Toast.LENGTH_SHORT).show();
                        }
                    } catch (IOException e) {
                        Toast.makeText(getApplicationContext(), "TTS synth: File not created:" + audiofile.getAbsolutePath(), Toast.LENGTH_SHORT).show();
                    }
                    if (audiofile.length() > 0) {
                        Toast.makeText(getApplicationContext(), "Saved", Toast.LENGTH_SHORT).show();
                    } else {
                        Toast.makeText(getApplicationContext(), "TTS synth: Audio not generated: " + audiofile.getAbsolutePath(), Toast.LENGTH_SHORT).show();
                    }
                } else {
                    Toast.makeText(getApplicationContext(), "Please, enter a message", Toast.LENGTH_SHORT).show();
                }
            }
        });
    }

    @Override
    protected void onDestroy() {
        if (ttsService != null) {
            ttsService.stop();
            ttsService.shutdown();
        }
        super.onDestroy();
    }

}
