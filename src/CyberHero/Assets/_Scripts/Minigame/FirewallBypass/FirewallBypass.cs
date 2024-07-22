using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FirewallBypass : MonoBehaviour
{
    public Button[] packetButtons; 
    public TextMeshProUGUI firewallRulesText;
    public TextMeshProUGUI resultText;

    public Button closeTerminalButton;

    private List<string> firewallRules = new List<string>()
    {
        "IP 192.168.1.1 BLOCKED",
        "Port 80 BLOCKED",
        "Payload 'Malware' BLOCKED"
    };

    private List<string> packetData = new List<string>()
    {
        "IP: 192.168.1.1, Port: 80, Payload: 'Hello'",
        "IP: 10.0.0.2, Port: 22, Payload: 'Data'",
        "IP: 192.168.1.1, Port: 22, Payload: 'Hello'",
        "IP: 10.0.0.2, Port: 80, Payload: 'Malware'"
    };

    void OnEnable()
    {
        SetupGame();
    }

    void SetupGame()
    {
        
        resultText.text = "";

     
        firewallRulesText.text = "Firewall Rules:\n";
        foreach (string rule in firewallRules)
        {
            firewallRulesText.text += rule + "\n";
        }

        
        for (int i = 0; i < packetButtons.Length; i++)
        {
            int index = i;
            packetButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = packetData[i];
            packetButtons[i].onClick.AddListener(() => SelectPacket(index));
        }
    }

    void SelectPacket(int index)
    {
        
        string selectedPacket = packetData[index];

        bool isBlocked = false;
        foreach (string rule in firewallRules)
        {
            if (selectedPacket.Contains(rule.Split(' ')[1]))
            {
                isBlocked = true;
                break;
            }
        }

        if (isBlocked)
        {
            resultText.text = "Packet Blocked by Firewall!";
        }
        else
        {
            resultText.text = "Packet Bypassed Firewall!";
            closeTerminalButton.enabled = true;
        }
    }
}
